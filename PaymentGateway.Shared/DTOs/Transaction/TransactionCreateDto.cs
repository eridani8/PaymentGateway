using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Shared.DTOs.Transaction;

public class TransactionCreateDto
{
    public string PaymentData { get; set; } = string.Empty;
    public TransactionSource Source { get; set; }
    public decimal ExtractedAmount { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string? RawMessage { get; set; }
}