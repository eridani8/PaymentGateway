using Microsoft.Extensions.Logging;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Application.Interfaces;

public interface IExpiredPaymentHandler
{
    Task HandleExpiredPayments(IUnitOfWork unit);
}