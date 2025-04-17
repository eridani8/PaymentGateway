using PaymentGateway.Core.Entities;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Core.Interfaces;

public interface ITransactionEntity
{
    Guid? RequisiteId { get; set; }
    RequisiteEntity? Requisite { get; set; }
    Guid? PaymentId { get; set; }
    PaymentEntity? Payment { get; set; }
    TransactionSource Source { get; set; }
    decimal ExtractedAmount { get; set; }
    DateTime ReceivedAt { get; set; }
    string? RawMessage { get; set; }
}