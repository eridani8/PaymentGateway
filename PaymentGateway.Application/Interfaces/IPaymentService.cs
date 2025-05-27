using System.Security.Claims;
using PaymentGateway.Core.Entities;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Application.Results;

namespace PaymentGateway.Application.Interfaces;

public interface IPaymentService
{
    Task<Result<PaymentDto>> CreatePayment(PaymentCreateDto dto, ClaimsPrincipal userClaim);
    Task<Result<PaymentEntity>> ManualConfirmPayment(PaymentManualConfirmDto dto, Guid currentUserId);
    Task<Result<PaymentEntity>> CancelPayment(PaymentCancelDto dto, Guid currentUserId);
    Task<Result<IEnumerable<PaymentDto>>> GetAllPayments();
    Task<Result<IEnumerable<PaymentDto>>> GetUserPayments(Guid userId);
    Task<Result<PaymentDto>> GetPaymentById(Guid id);
    Task<Result<PaymentDto>> DeletePayment(Guid id);
}