﻿using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Enums;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Application.Services;

public class GatewayHandler(ILogger<GatewayHandler> logger, ICache cache) : IGatewayHandler
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
                if (requisite is { Status: RequisiteStatus.Pending, PaymentId: not null }) continue;

                var status = requisite.DetermineStatus(now, nowTimeOnly);

                if (status != requisite.Status)
                {
                    logger.LogInformation("Статус реквизита {requisiteId} изменен с {oldStatus} на {newStatus}", requisite.Id, requisite.Status.ToString(), status.ToString());
                    requisite.Status = status;
                    unit.RequisiteRepository.Update(requisite);
                }
                
                if (requisite.LastFundsResetAt.Date < now.Date)
                {
                    logger.LogInformation("Сброс полученных средств с реквизита {requisiteId}", requisite.Id);
                    requisite.ReceivedFunds = 0;
                    requisite.LastFundsResetAt = now;
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
            
            var requisite = freeRequisites.FirstOrDefault(r => r.MaxAmount >= payment.Amount);
            if (requisite is null)
            {
                logger.LogWarning("Нет подходящего реквизита для платежа {paymentId} с суммой {amount}", payment.Id, payment.Amount);
                continue;
            }
            freeRequisites.Remove(requisite);
            
            requisite.AssignToPayment(payment);
            payment.MarkAsPending(requisite);

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
                    expiredPayment.Requisite.Status = RequisiteStatus.Active;
                }
                logger.LogInformation("Платеж {paymentId} на сумму {amount} отменен из-за истечения срока ожидания", expiredPayment.Id, expiredPayment.Amount);
            }
        
            unit.PaymentRepository.DeletePayments(expiredPayments);
        }
    }
}