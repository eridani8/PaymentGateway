using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Shared.DTOs.Payment;

public class PaymentDto
{
    public Guid Id { get; init; }
    public Guid ExternalPaymentId { get; init; }
    public decimal Amount { get; init; }
    public Guid? RequisiteId { get; init; }
    public RequisiteDto? Requisite { get; init; }
    public PaymentStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public Guid? TransactionId { get; init; }
    public Guid? ManualConfirmUserId { get; init; }
    public Guid? CanceledByUserId { get; init; }
}