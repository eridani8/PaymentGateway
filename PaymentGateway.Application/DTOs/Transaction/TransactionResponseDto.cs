using PaymentGateway.Core.Enums;

namespace PaymentGateway.Application.DTOs.Transaction;

public class TransactionResponseDto
{
    public required Guid Id { get; init; }
    public required Guid? RequisiteId { get; init; }
    public required Guid? PaymentId { get; init; }
    public required TransactionSource Source { get; init; }
    public required decimal ExtractedAmount { get; init; }
    public required DateTime ReceivedAt { get; init; }
    public required string? RawMessage { get; init; }
}