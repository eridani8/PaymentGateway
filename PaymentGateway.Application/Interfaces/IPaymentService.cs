using PaymentGateway.Shared.DTOs.Payment;

namespace PaymentGateway.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto?> CreatePayment(PaymentCreateDto dto);
    Task<PaymentDto?> ManualConfirmPayment(PaymentManualConfirmDto dto, Guid currentUserId);
    Task<PaymentDto?> CancelPayment(PaymentCancelDto dto, Guid currentUserId);
    Task<IEnumerable<PaymentDto>> GetAllPayments();
    Task<IEnumerable<PaymentDto>> GetUserPayments(Guid userId);
    Task<PaymentDto?> GetPaymentById(Guid id);
    Task<PaymentDto?> DeletePayment(Guid id);
}