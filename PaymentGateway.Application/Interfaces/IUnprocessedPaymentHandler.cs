using Microsoft.Extensions.Logging;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Application.Interfaces;

public interface IUnprocessedPaymentHandler
{
    Task HandleUnprocessedPayments(IUnitOfWork unit);
}