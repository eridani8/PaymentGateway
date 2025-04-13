using PaymentGateway.Application.DTOs.Payment;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto> CreatePayment(PaymentCreateDto dto);
    Task<IEnumerable<PaymentDto>> GetAllPayments();
    Task<PaymentDto?> GetPaymentById(Guid id);
    Task<bool> DeletePayment(Guid id);
}