using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Enums;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Application.Services;

public class UnprocessedPaymentHandler(ILogger<UnprocessedPaymentHandler> logger) : IUnprocessedPaymentHandler
{
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

            var requisite = freeRequisites.FirstOrDefault(r => r.MaxAmount <= payment.Amount);
            if (requisite is null)
            {
                logger.LogWarning("Нет подходящего реквизита для платежа {paymentId} с суммой {amount}", payment.Id, payment.Amount);
                continue;
            }

            freeRequisites.Remove(requisite);

            payment.RequisiteId = requisite.Id;
            payment.Status = PaymentStatus.Pending;

            requisite.CurrentPaymentId = payment.Id;
            requisite.LastOperationTime = DateTime.UtcNow;

            logger.LogInformation("Платеж {payment} назначен реквизиту {requisite}", payment.Id, requisite.Id);
        }

        await unit.Commit();
    }
}