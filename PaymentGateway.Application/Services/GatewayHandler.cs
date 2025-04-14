using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared.Enums;
using PaymentGateway.Shared.Interfaces;

namespace PaymentGateway.Application.Services;

public class GatewayHandler(
    ILogger<GatewayHandler> logger,
    ICache cache,
    INotificationService notificationService)
    : IGatewayHandler
{
    public async Task HandleRequisites(IUnitOfWork unit)
    {
        var requisites = await unit.RequisiteRepository.GetAll();
        var now = DateTime.UtcNow;
        var nowTimeOnly = TimeOnly.FromDateTime(now);
        var hasChanges = false;
        
        foreach (var requisite in requisites)
        {
            try
            {
                if (requisite is { Status: RequisiteStatus.Pending, PaymentId: not null }) continue;

                var status = !requisite.IsActive ? RequisiteStatus.Inactive : requisite.DetermineStatus(now, nowTimeOnly);

                if (status != requisite.Status)
                {
                    logger.LogInformation("Статус реквизита {requisiteId} изменен с {oldStatus} на {newStatus}", requisite.Id, requisite.Status.ToString(), status.ToString());
                    requisite.Status = status;
                    unit.RequisiteRepository.Update(requisite);
                    hasChanges = true;
                }
                
                if (requisite.LastFundsResetAt.Date < now.Date)
                {
                    logger.LogInformation("Сброс полученных средств с реквизита {requisiteId}", requisite.Id);
                    requisite.ReceivedFunds = 0;
                    requisite.LastFundsResetAt = now;
                    unit.RequisiteRepository.Update(requisite);
                    hasChanges = true;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка при обработке реквизита {requisiteId}", requisite.Id);
            }
        }

        if (hasChanges)
        {
            await notificationService.NotifyRequisiteUpdated();
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
        
        var hasChanges = false;
        foreach (var payment in unprocessedPayments)
        {
            if (freeRequisites.Count == 0)
            {
                logger.LogWarning(noAvailableRequisites);
                return;
            }
            
            var requisite = freeRequisites.FirstOrDefault(r => r.MaxAmount >= payment.Amount);
            if (requisite is null)
            {
                logger.LogWarning("Нет подходящего реквизита для платежа {paymentId} с суммой {amount}", payment.Id, payment.Amount);
                continue;
            }
            freeRequisites.Remove(requisite);
            
            requisite.AssignToPayment(payment);
            payment.MarkAsPending(requisite);
            hasChanges = true;

            logger.LogInformation("Платеж {payment} назначен реквизиту {requisite}", payment.Id, requisite.Id);
        }

        if (hasChanges)
        {
            await notificationService.NotifyPaymentUpdated();
            await notificationService.NotifyRequisiteUpdated();
        }
    }
    
    public async Task HandleExpiredPayments(IUnitOfWork unit)
    {
        var expiredPayments = await unit.PaymentRepository.GetExpiredPayments();
        if (expiredPayments.Count > 0)
        {
            var hasChanges = false;
            foreach (var expiredPayment in expiredPayments)
            {
                if (expiredPayment.Requisite != null)
                {
                    expiredPayment.Requisite.Status = RequisiteStatus.Active;
                    hasChanges = true;
                }
                
                logger.LogInformation("Платеж {paymentId} на сумму {amount} отменен из-за истечения срока ожидания", expiredPayment.Id, expiredPayment.Amount);
            }
        
            unit.PaymentRepository.DeletePayments(expiredPayments);

            if (hasChanges)
            {
                await notificationService.NotifyPaymentUpdated();
                await notificationService.NotifyRequisiteUpdated();
            }
        }
    }
}