using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Application.Interfaces;

public interface IPaymentHandler
{
    Task HandleExpiredPayments(IUnitOfWork unit);
    Task HandleUnprocessedPayments(IUnitOfWork unit);
}