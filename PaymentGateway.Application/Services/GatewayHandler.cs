using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Interfaces;

namespace PaymentGateway.Application.Services;

public class GatewayHandler(
    ILogger<GatewayHandler> logger,
    ICache cache,
    INotificationService notificationService,
    IMapper mapper)
    : IGatewayHandler
{
    public async Task HandleRequisites(IUnitOfWork unit)
    {
        var requisites = await unit.RequisiteRepository.GetAll();
        var now = DateTime.UtcNow;
        var nowTimeOnly = TimeOnly.FromDateTime(now);
        
        foreach (var requisite in requisites)
        {
            try
            {
                requisite.ProcessStatus(now, nowTimeOnly, out var status);

                if (requisite.Status != status)
                {
                    logger.LogInformation("Статус реквизита {requisiteId} изменен с {oldStatus} на {newStatus}", requisite.Id, requisite.Status.ToString(), status.ToString());
                    requisite.Status = status;
                    unit.RequisiteRepository.Update(requisite);
                    await notificationService.NotifyRequisiteUpdated(mapper.Map<RequisiteDto>(requisite));
                }
                
                var todayDate = now.ToLocalTime().Date;
                var lastResetDate = requisite.LastFundsResetAt.ToLocalTime().Date;
                var resetCacheKey = $"funds_reset:{requisite.Id}:{todayDate:yyyy-MM-dd}";
                
                if (lastResetDate < todayDate && !cache.Exists(resetCacheKey))
                {
                    logger.LogInformation("Сброс полученных средств с реквизита {requisiteId}", requisite.Id);
                    requisite.ReceivedFunds = 0;
                    requisite.LastFundsResetAt = now;
                    unit.RequisiteRepository.Update(requisite);
                    cache.Set(resetCacheKey, TimeSpan.FromHours(25));
                    await notificationService.NotifyRequisiteUpdated(mapper.Map<RequisiteDto>(requisite));
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка при обработке реквизита {requisiteId}", requisite.Id);
            }
        }
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
            if (freeRequisites.Count == 0)
            {
                logger.LogWarning(noAvailableRequisites);
                return;
            }
            
            var requisite = freeRequisites.FirstOrDefault(r => 
                r.MaxAmount >= payment.Amount && 
                (r.MaxAmount - r.ReceivedFunds) >= payment.Amount
            );
            if (requisite is null)
            {
                logger.LogWarning("Нет подходящего реквизита для платежа {paymentId} с суммой {amount}", payment.Id, payment.Amount);
                continue;
            }
            freeRequisites.Remove(requisite);
            
            requisite.AssignToPayment(payment);
            payment.MarkAsPending(requisite);

            await notificationService.NotifyPaymentUpdated(mapper.Map<PaymentDto>(payment));
            await notificationService.NotifyRequisiteUpdated(mapper.Map<RequisiteDto>(requisite));

            logger.LogInformation("Платеж {payment} назначен реквизиту {requisite}", payment.Id, requisite.Id);
        }
    }
    
    public async Task HandleExpiredPayments(IUnitOfWork unit)
    {
        var expiredPayments = await unit.PaymentRepository.GetExpiredPayments();
        if (expiredPayments.Count > 0)
        {
            foreach (var expiredPayment in expiredPayments)
            {
                if (expiredPayment.Requisite != null)
                {
                    expiredPayment.Requisite.ReleaseWithoutPayment();
                    await notificationService.NotifyRequisiteUpdated(mapper.Map<RequisiteDto>(expiredPayment.Requisite));
                }
                await notificationService.NotifyPaymentUpdated(mapper.Map<PaymentDto>(expiredPayment));
                logger.LogInformation("Платеж {paymentId} на сумму {amount} отменен из-за истечения срока ожидания", expiredPayment.Id, expiredPayment.Amount);
            }
        
            unit.PaymentRepository.DeletePayments(expiredPayments);
        }
    }
    
    public async Task HandleUserFundsReset(UserManager<UserEntity> userManager)
    {
        var now = DateTime.UtcNow;
        var todayDate = now.ToLocalTime().Date;
        
        var globalResetKey = $"global_user_funds_reset:{todayDate:yyyy-MM-dd}";
        if (cache.Exists(globalResetKey))
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
        logger.LogInformation("Завершен процесс ежедневного сброса средств пользователей. Обработано: {count} пользователей", resetCount);
    }
}