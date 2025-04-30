using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Web.Services;

namespace PaymentGateway.Web.Interfaces;

public interface IPaymentService
{
    Task<Guid?> CreatePayment(PaymentCreateDto dto);
    Task<Response> ManualConfirmPayment(Guid id);
    Task<Response> CancelPayment(Guid id);
    Task<List<PaymentDto>> GetPayments();
    Task<List<PaymentDto>> GetUserPayments();
    Task<List<PaymentDto>> GetPaymentsByUserId(Guid userId);
    Task<PaymentDto?> GetPaymentById(Guid id);
}