using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Web.Services;

namespace PaymentGateway.Web.Interfaces;

public interface IPaymentService
{
    Task<Response> ManualConfirmPayment(Guid id);
    Task<List<PaymentDto>> GetPayments();
    Task<List<PaymentDto>> GetUserPayments();
}