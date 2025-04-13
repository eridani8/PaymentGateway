using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Shared.DTOs.Transaction;

public class TransactionDto
{
    public required Guid Id { get; init; }
    public required Guid? RequisiteId { get; init; }
    public required Guid? PaymentId { get; init; }
    public required TransactionSource Source { get; init; }
    public required decimal ExtractedAmount { get; init; }
    public required DateTime ReceivedAt { get; init; }
    public required string? RawMessage { get; init; }
}