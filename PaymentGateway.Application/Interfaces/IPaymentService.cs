using PaymentGateway.Shared.DTOs.Payment;

namespace PaymentGateway.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto> CreatePayment(PaymentCreateDto dto);
    Task<IEnumerable<PaymentDto>> GetAllPayments();
    Task<PaymentDto?> GetPaymentById(Guid id);
    Task<bool> DeletePayment(Guid id);
}