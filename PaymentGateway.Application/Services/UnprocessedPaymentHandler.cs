using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Enums;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Application.Services;

public class UnprocessedPaymentHandler(ILogger<UnprocessedPaymentHandler> logger) : IUnprocessedPaymentHandler
{
    public async Task HandleUnprocessedPayments(IUnitOfWork unit, IRequisiteService requisiteService)
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

            var requisite = requisiteService.SelectRequisite(freeRequisites, payment);
            if (requisite is null)
            {
                payment.Handle = true;
                logger.LogWarning("Нет подходящего реквизита для платежа {paymentId} с суммой {amount}", payment.Id, payment.Amount);
                continue;
            }

            freeRequisites.Remove(requisite);
            
            requisiteService.PendingRequisite(requisite, payment);

            logger.LogInformation("Платеж {payment} назначен реквизиту {requisite}", payment.Id, requisite.Id);
        }

        await unit.Commit();
    }
}