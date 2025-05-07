using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Interfaces;
using PaymentGateway.Infrastructure.Repositories;
using PaymentGateway.Infrastructure.Repositories.Cached;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Application.Services;

public class GatewayHandler(
    ILogger<GatewayHandler> logger,
    IMemoryCache cache,
    INotificationService notificationService,
    IMapper mapper)
    : IGatewayHandler
{
    public async Task HandleRequisites(IUnitOfWork unit)
    {
        var requisites = await unit.RequisiteRepository.GetAll();
        var now = DateTime.UtcNow;
        var nowTimeOnly = TimeOnly.FromDateTime(now);

        var needToCommit = false;

        foreach (var requisite in requisites)
        {
            try
            {
                requisite.ProcessStatus(now, nowTimeOnly, out var status);

                if (requisite.Status != status)
                {
                    logger.LogInformation("Статус реквизита {RequisiteId} изменен с {OldStatus} на {NewStatus}",
                        requisite.Id, requisite.Status.ToString(), status.ToString());
                    requisite.Status = status;
                    unit.RequisiteRepository.Update(requisite);
                    await notificationService.NotifyRequisiteUpdated(mapper.Map<RequisiteDto>(requisite));
                    needToCommit = true;
                }

                var todayDate = now.ToLocalTime().Date;
                var lastResetDate = requisite.LastDayFundsResetAt.ToLocalTime().Date;
                var resetCacheKey = $"funds_reset:{requisite.Id}:{todayDate:yyyy-MM-dd}";

                if (lastResetDate < todayDate && cache.Get(resetCacheKey) is null)
                {
                    logger.LogInformation("Сброс полученных средств с реквизита {RequisiteId}", requisite.Id);
                    requisite.DayReceivedFunds = 0;
                    requisite.DayOperationsCount = 0;
                    requisite.LastDayFundsResetAt = now;
                    cache.Set(resetCacheKey, TimeSpan.FromHours(25));
                    await notificationService.NotifyRequisiteUpdated(mapper.Map<RequisiteDto>(requisite));
                    needToCommit = true;
                }

                var currentMonth = new DateTime(now.Year, now.Month, 1);
                var lastMonthlyResetDate = new DateTime(requisite.LastMonthlyFundsResetAt.Year,
                    requisite.LastMonthlyFundsResetAt.Month, 1);
                var monthlyResetCacheKey = $"monthly_funds_reset:{requisite.Id}:{currentMonth:yyyy-MM}";

                if (lastMonthlyResetDate < currentMonth && cache.Get(monthlyResetCacheKey) is null)
                {
                    logger.LogInformation("Сброс полученных средств за месяц с реквизита {RequisiteId}", requisite.Id);
                    requisite.MonthReceivedFunds = 0;
                    requisite.LastMonthlyFundsResetAt = now;
                    cache.Set(monthlyResetCacheKey, TimeSpan.FromDays(32));
                    await notificationService.NotifyRequisiteUpdated(mapper.Map<RequisiteDto>(requisite));
                    needToCommit = true;
                }

                if (needToCommit)
                {
                    unit.RequisiteRepository.Update(requisite);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка при обработке реквизита {RequisiteId}", requisite.Id);
            }
        }

        if (needToCommit)
        {
            await unit.Commit();
        }
    }

    public async Task HandleUnprocessedPayments(IUnitOfWork unit)
    {
        var unprocessedPayments = await unit.PaymentRepository.GetUnprocessedPayments();

        if (unprocessedPayments.Count == 0) return;

        var activeRequisites = await unit.RequisiteRepository.GetActiveRequisites();

        if (activeRequisites.Count == 0)
        {
            const string noRequisitesCacheKey = "no_requisites_warning";
            if (cache.Get(noRequisitesCacheKey) is null)
            {
                logger.LogWarning("Нет свободных реквизитов для обработки платежей");
                cache.Set(noRequisitesCacheKey, 0, TimeSpan.FromMinutes(1));
            }

            return;
        }

        var needToCommit = false;
        foreach (var payment in unprocessedPayments)
        {
            try
            {
                var requisite = activeRequisites.FirstOrDefault(r =>
                    r.DayLimit >= payment.Amount &&
                    (r.DayLimit - r.DayReceivedFunds) >= payment.Amount &&
                    (r.MonthLimit == 0 || r.MonthLimit >= payment.Amount) &&
                    (r.MonthLimit == 0 || (r.MonthLimit - r.MonthReceivedFunds) >= payment.Amount)
                );
                if (requisite is null)
                {
                    var noSuitableRequisiteCacheKey = $"no_suitable_requisite_warning:{payment.Id}";
                    if (cache.Get(noSuitableRequisiteCacheKey) is null)
                    {
                        logger.LogWarning("Нет подходящего реквизита для платежа {PaymentId} с суммой {Amount}",
                            payment.Id,
                            payment.Amount);
                        cache.Set(noSuitableRequisiteCacheKey, 0, TimeSpan.FromMinutes(1));
                    }
                    continue;
                }

                if (!activeRequisites.Contains(requisite))
                {
                    continue;
                }

                activeRequisites.Remove(requisite);
                requisite.AssignToPayment(payment);
                payment.MarkAsPending(requisite);
                
                needToCommit = true;
                
                unit.RequisiteRepository.Update(requisite);
                unit.PaymentRepository.Update(payment);

                var paymentDto = mapper.Map<PaymentDto>(payment);
                var requisiteDto = mapper.Map<RequisiteDto>(requisite);

                await notificationService.NotifyPaymentUpdated(paymentDto);
                await notificationService.NotifyRequisiteUpdated(requisiteDto);

                logger.LogInformation("Платеж {Payment} назначен реквизиту {Requisite}", payment.Id, requisite.Id);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка при обработке платежа {PaymentId}", payment.Id);
            }
        }
        
        if (needToCommit)
        {
            await unit.Commit();
        }
    }

    public async Task HandleExpiredPayments(IUnitOfWork unit)
    {
        var expiredPayments = await unit.PaymentRepository.GetExpiredPayments();
        if (expiredPayments.Count > 0)
        {
            var needToCommit = false;
            foreach (var expiredPayment in expiredPayments)
            {
                var requisiteUserId = Guid.Empty;
                if (expiredPayment.Requisite is { } requisite)
                {
                    requisiteUserId = requisite.Id;
                    requisite.ReleaseWithoutPayment();
                    await notificationService.NotifyRequisiteUpdated(mapper.Map<RequisiteDto>(requisite));
                    unit.RequisiteRepository.Update(requisite);
                }

                unit.PaymentRepository.Delete(expiredPayment);
                await notificationService.NotifyPaymentDeleted(expiredPayment.Id, requisiteUserId == Guid.Empty ? null : requisiteUserId);
                logger.LogInformation("Платеж {PaymentId} на сумму {Amount} отменен из-за истечения срока ожидания", expiredPayment.Id, expiredPayment.Amount);
                needToCommit = true;
            }

            if (needToCommit)
            {
                await unit.Commit();
            }
        }
    }

    public async Task HandleUserFundsReset(UserManager<UserEntity> userManager)
    {
        var now = DateTime.UtcNow;
        var todayDate = now.ToLocalTime().Date;

        var globalResetKey = $"global_user_funds_reset:{todayDate:yyyy-MM-dd}";
        if (cache.Get(globalResetKey) is not null)
        {
            return;
        }

        logger.LogInformation("Начат процесс ежедневного сброса средств пользователей");
        var users = await userManager.Users.ToListAsync();
        var resetCount = 0;

        foreach (var user in users)
        {
            try
            {
                var lastResetDate = user.LastFundsResetAt.ToLocalTime().Date;
                if (lastResetDate < todayDate)
                {
                    logger.LogInformation("Сброс полученных средств пользователя {UserId}", user.Id);
                    user.ReceivedDailyFunds = 0;
                    user.LastFundsResetAt = now;
                    await userManager.UpdateAsync(user);
                    await notificationService.NotifyUserUpdated(mapper.Map<UserDto>(user));
                    resetCount++;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка при обработке пользователя {UserId}", user.Id);
            }
        }

        cache.Set(globalResetKey, TimeSpan.FromHours(25));
        logger.LogInformation(
            "Завершен процесс ежедневного сброса средств пользователей. Обработано: {count} пользователей", resetCount);
    }
}