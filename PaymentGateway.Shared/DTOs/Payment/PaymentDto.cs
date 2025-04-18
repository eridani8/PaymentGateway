using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Shared.DTOs.Payment;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid ExternalPaymentId { get; set; }
    public decimal Amount { get; set; }
    public Guid? RequisiteId { get; set; }
    public RequisiteDto? Requisite { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Guid? TransactionId { get; set; }
    public Guid? ManualConfirmUserId { get; set; }
    public Guid? CanceledByUserId { get; set; }
}