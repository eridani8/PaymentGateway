using PaymentGateway.Application.DTOs.Payment;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponseDto> CreatePayment(PaymentCreateDto dto);
    Task<IEnumerable<PaymentResponseDto>> GetAllPayments();
    Task<PaymentResponseDto?> GetPaymentById(Guid id);
    Task<bool> DeletePayment(Guid id);
}