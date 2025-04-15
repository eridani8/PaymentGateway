﻿using AutoMapper;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.Enums;
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
                if (requisite is { Status: RequisiteStatus.Pending, PaymentId: not null }) continue;

                var status = !requisite.IsActive ? RequisiteStatus.Inactive : requisite.DetermineStatus(now, nowTimeOnly);

                if (status != requisite.Status)
                {
                    logger.LogInformation("Статус реквизита {requisiteId} изменен с {oldStatus} на {newStatus}", requisite.Id, requisite.Status.ToString(), status.ToString());
                    requisite.Status = status;
                    unit.RequisiteRepository.Update(requisite);
                    await notificationService.NotifyRequisiteUpdated(mapper.Map<RequisiteDto>(requisite));
                }
                
                if (requisite.LastFundsResetAt.Date < now.Date)
                {
                    logger.LogInformation("Сброс полученных средств с реквизита {requisiteId}", requisite.Id);
                    requisite.ReceivedFunds = 0;
                    requisite.LastFundsResetAt = now;
                    unit.RequisiteRepository.Update(requisite);
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
            
            var requisite = freeRequisites.FirstOrDefault(r => r.MaxAmount >= payment.Amount);
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
}