using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Interfaces;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Interfaces;

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
        var requisites = await unit.RequisiteRepository.GetAllTracked();
        var now = DateTime.UtcNow;
        var nowTimeOnly = TimeOnly.FromDateTime(now);

        foreach (var requisite in requisites)
        {
            try
            {
                requisite.ProcessStatus(now, nowTimeOnly, out var status);

                if (requisite.Status != status)
                {
                    logger.LogInformation("Статус реквизита {requisiteId} изменен с {oldStatus} на {newStatus}",
                        requisite.Id, requisite.Status.ToString(), status.ToString());
                    requisite.Status = status;
                    unit.RequisiteRepository.Update(requisite);
                    await notificationService.NotifyRequisiteUpdated(mapper.Map<RequisiteDto>(requisite));
                }

                var todayDate = now.ToLocalTime().Date;
                var lastResetDate = requisite.LastDayFundsResetAt.ToLocalTime().Date;
                var resetCacheKey = $"funds_reset:{requisite.Id}:{todayDate:yyyy-MM-dd}";

                if (lastResetDate < todayDate && cache.Get(resetCacheKey) is null)
                {
                    logger.LogInformation("Сброс полученных средств с реквизита {requisiteId}", requisite.Id);
                    requisite.DayReceivedFunds = 0;
                    requisite.DayOperationsCount = 0;
                    requisite.LastDayFundsResetAt = now;
                    unit.RequisiteRepository.Update(requisite);
                    cache.Set(resetCacheKey, TimeSpan.FromHours(25));
                    await notificationService.NotifyRequisiteUpdated(mapper.Map<RequisiteDto>(requisite));
                }

                var currentMonth = new DateTime(now.Year, now.Month, 1);
                var lastMonthlyResetDate = new DateTime(requisite.LastMonthlyFundsResetAt.Year, requisite.LastMonthlyFundsResetAt.Month, 1);
                var monthlyResetCacheKey = $"monthly_funds_reset:{requisite.Id}:{currentMonth:yyyy-MM}";

                if (lastMonthlyResetDate < currentMonth && cache.Get(monthlyResetCacheKey) is null)
                {
                    logger.LogInformation("Сброс полученных средств за месяц с реквизита {requisiteId}", requisite.Id);
                    requisite.MonthReceivedFunds = 0;
                    requisite.LastMonthlyFundsResetAt = now;
                    unit.RequisiteRepository.Update(requisite);
                    cache.Set(monthlyResetCacheKey, TimeSpan.FromDays(32));
                    await notificationService.NotifyRequisiteUpdated(mapper.Map<RequisiteDto>(requisite));
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка при обработке реквизита {requisiteId}", requisite.Id);
            }
        }

        await unit.Commit();
    }

    public async Task HandleUnprocessedPayments(IUnitOfWork unit)
    {
        var unprocessedPayments = await unit.PaymentRepository.GetUnprocessedPayments();

        if (unprocessedPayments.Count == 0) return;

        var freeRequisites = await unit.RequisiteRepository.GetFreeRequisites();

        const string noAvailableRequisites = "Нет свободных реквизитов для обработки платежей";
        if (freeRequisites.Count == 0)
        {
            logger.LogWarning(noAvailableRequisites);
            return;
        }

        foreach (var payment in unprocessedPayments)
        {
            try
            {
                if (freeRequisites.Count == 0)
                {
                    logger.LogWarning(noAvailableRequisites);
                    return;
                }

                var requisite = freeRequisites.FirstOrDefault(r =>
                    r.DayLimit >= payment.Amount &&
                    (r.DayLimit - r.DayReceivedFunds) >= payment.Amount &&
                    (r.MonthLimit == 0 || r.MonthLimit >= payment.Amount) &&
                    (r.MonthLimit == 0 || (r.MonthLimit - r.MonthReceivedFunds) >= payment.Amount)
                );
                if (requisite is null)
                {
                    logger.LogWarning("Нет подходящего реквизита для платежа {paymentId} с суммой {amount}", payment.Id,
                        payment.Amount);
                    continue;
                }

                freeRequisites.Remove(requisite);

                requisite.AssignToPayment(payment);
                payment.MarkAsPending(requisite);
                await unit.Commit();

                var paymentDto = mapper.Map<PaymentDto>(payment);
                var requisiteDto = mapper.Map<RequisiteDto>(requisite);

                await notificationService.NotifyPaymentUpdated(paymentDto);
                await notificationService.NotifyRequisiteUpdated(requisiteDto);

                logger.LogInformation("Платеж {payment} назначен реквизиту {requisite}", payment.Id, requisite.Id);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка при обработке платежа {PaymentId}", payment.Id);
            }
        }
    }

    public async Task HandleExpiredPayments(IUnitOfWork unit)
    {
        var expiredPayments = await unit.PaymentRepository.GetExpiredPayments();
        if (expiredPayments.Count > 0)
        {
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
                await notificationService.NotifyPaymentDeleted(expiredPayment.Id,
                    requisiteUserId == Guid.Empty ? null : requisiteUserId);
                logger.LogInformation("Платеж {paymentId} на сумму {amount} отменен из-за истечения срока ожидания",
                    expiredPayment.Id, expiredPayment.Amount);
            }
        }

        await unit.Commit();
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
                    logger.LogInformation("Сброс полученных средств пользователя {userId}", user.Id);
                    user.ReceivedDailyFunds = 0;
                    user.LastFundsResetAt = now;
                    await userManager.UpdateAsync(user);
                    await notificationService.NotifyUserUpdated(mapper.Map<UserDto>(user));
                    resetCount++;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка при обработке пользователя {userId}", user.Id);
            }
        }

        cache.Set(globalResetKey, TimeSpan.FromHours(25));
        logger.LogInformation(
            "Завершен процесс ежедневного сброса средств пользователей. Обработано: {count} пользователей", resetCount);
    }
}