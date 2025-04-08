using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Enums;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Application.Services;

public class GatewayHandler(ILogger<GatewayHandler> logger) : IGatewayHandler
{
    public async Task HandleExpiredPayments(IUnitOfWork unit)
    {
        var expiredPayments = await unit.PaymentRepository.GetExpiredPayments();
        if (expiredPayments.Count > 0)
        {
            foreach (var expiredPayment in expiredPayments)
            {
                logger.LogInformation("Платеж {payment} просрочен, и был удален", expiredPayment.Id);
            }
        
            unit.PaymentRepository.DeletePayments(expiredPayments);
        }
    }

    public async Task HandleRequisites(IUnitOfWork unit)
    {
        var requisites = await unit.RequisiteRepository.GetAll();
        var now = DateTime.UtcNow;
        var nowTimeOnly = TimeOnly.FromDateTime(now);
        foreach (var requisite in requisites)
        {
            try
            {
                if (!requisite.IsActive) continue;
                
                var inWorkingTime = requisite.IsWorkingTime(nowTimeOnly);
                var cooldownOver = requisite.IsCooldownOver(now);
                RequisiteStatus newStatus;
                
                if (inWorkingTime)
                {
                    newStatus = cooldownOver ? RequisiteStatus.Active : RequisiteStatus.Cooldown;
                }
                else
                {
                    newStatus = RequisiteStatus.Inactive;
                }

                if (requisite.Status != RequisiteStatus.Pending && newStatus == requisite.Status) continue;
                
                requisite.Status = newStatus;
                unit.RequisiteRepository.Update(requisite);
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

        foreach (var payment in unprocessedPayments)
        {
            if (freeRequisites.Count == 0)
            {
                logger.LogWarning("Нет свободных реквизитов для обработки платежей");
                break;
            }

            var requisite = freeRequisites.FirstOrDefault(r => r.MaxAmount >= payment.Amount);
            if (requisite is null)
            {
                logger.LogWarning("Нет подходящего реквизита для платежа {paymentId} с суммой {amount}", payment.Id, payment.Amount);
                continue;
            }
            freeRequisites.Remove(requisite);
            
            requisite.AssignToPayment(payment.Id);
            payment.MarkAsPending(requisite.Id);

            logger.LogInformation("Платеж {payment} назначен реквизиту {requisite}", payment.Id, requisite.Id);
        }
    }
}