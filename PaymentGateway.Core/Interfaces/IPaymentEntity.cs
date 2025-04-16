using PaymentGateway.Core.Entities;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Core.Interfaces;

public interface IPaymentEntity
{
    Guid Id { get; init; }
    Guid ExternalPaymentId { get; init; }
    Guid? UserId { get; init; }
    decimal Amount { get; init; }
    Guid? RequisiteId { get; set; }
    RequisiteEntity? Requisite { get; set; }
    PaymentStatus Status { get; set; }
    DateTime CreatedAt { get; init; }
    DateTime? ProcessedAt { get; set; }
    DateTime? ExpiresAt { get; set; }
    Guid? TransactionId { get; set; }
    TransactionEntity? Transaction { get; set; }
    void MarkAsPending(RequisiteEntity requisite);
    void ConfirmTransaction(TransactionEntity transaction);
    void ManualConfirm();
}