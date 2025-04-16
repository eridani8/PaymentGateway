using PaymentGateway.Web.Services;

namespace PaymentGateway.Web.Interfaces;

public interface IPaymentService
{
    Task<Response> ManualConfirmPayment(Guid id);
}