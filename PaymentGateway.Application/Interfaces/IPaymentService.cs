using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.Validations.Validators.Payment;

namespace PaymentGateway.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto?> CreatePayment(PaymentCreateDto dto);
    Task<PaymentDto?> ManualConfirmPayment(PaymentManualConfirmDto dto, Guid currentUserId);
    Task<IEnumerable<PaymentDto>> GetAllPayments();
    Task<PaymentDto?> GetPaymentById(Guid id);
    Task<PaymentDto?> DeletePayment(Guid id);
}