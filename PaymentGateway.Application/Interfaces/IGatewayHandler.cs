using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Application.Interfaces;

public interface IGatewayHandler
{
    Task HandleRequisites(IUnitOfWork unit);
    Task HandleUnprocessedPayments(IUnitOfWork unit);
    Task HandleExpiredPayments(IUnitOfWork unit);
}