using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Application.Services;

public class ExpiredPaymentHandler(ILogger<ExpiredPaymentHandler> logger) : IExpiredPaymentHandler
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

            await unit.PaymentRepository.DeletePayments(expiredPayments);
            await unit.Commit();
        }
    }
}