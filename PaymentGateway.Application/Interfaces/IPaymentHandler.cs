using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Application.Interfaces;

public interface IPaymentHandler
{
    Task HandleUnprocessedPayments(IUnitOfWork unit);
    Task HandleExpiredPayments(IUnitOfWork unit);
}