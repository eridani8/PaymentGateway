using PaymentGateway.Application.DTOs;

namespace PaymentGateway.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
}