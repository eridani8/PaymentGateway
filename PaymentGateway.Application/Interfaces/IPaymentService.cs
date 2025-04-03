using PaymentGateway.Application.DTOs.Payment;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Application.Interfaces;

public interface IPaymentService
{
    Task<List<PaymentEntity>> GetExpiredPayments();
    Task<List<PaymentEntity>> GetUnprocessedPayments();
    Task<PaymentResponseDto> CreatePayment(PaymentCreateDto dto);
    Task<IEnumerable<PaymentResponseDto>> GetAllPayments();
    Task<PaymentResponseDto?> GetPaymentById(Guid id);
    Task<bool> DeletePayment(Guid id);
    Task DeletePayments(IEnumerable<PaymentEntity> entities);
}