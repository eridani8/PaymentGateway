using PaymentGateway.Core.Entities;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Core.Interfaces;

public interface ITransactionEntity
{
    Guid Id { get; init; }
    Guid? RequisiteId { get; init; }
    RequisiteEntity? Requisite { get; init; }
    Guid? PaymentId { get; init; }
    PaymentEntity? Payment { get; init; }
    TransactionSource Source { get; init; }
    decimal ExtractedAmount { get; init; }
    DateTime ReceivedAt { get; init; }
    string? RawMessage { get; init; }
}