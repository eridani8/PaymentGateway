using PaymentGateway.Application.DTOs.Payment;

namespace PaymentGateway.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponseDto> CreatePayment(PaymentCreateDto dto);
    Task<IEnumerable<PaymentResponseDto>> GetAllPayments();
    Task<PaymentResponseDto?> GetPaymentById(Guid id);
    // Task<bool> UpdatePayment(Guid id, PaymentCreateDto dto); // ~
    Task<bool> DeletePayment(Guid id);
}