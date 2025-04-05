using PaymentGateway.Core.Enums;

namespace PaymentGateway.Application.DTOs.Transaction;

public class TransactionCreateDto
{
    public required Guid RequisiteId { get; init; }
    public required TransactionSource Source { get; init; }
    public required decimal ExtractedAmount { get; init; }
    public required DateTime ReceivedAt { get; init; }
}