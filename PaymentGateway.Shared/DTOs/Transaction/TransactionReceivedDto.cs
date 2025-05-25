using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Shared.DTOs.Transaction;

public class TransactionReceivedDto
{
    public TransactionSource Source { get; set; }
    public decimal ExtractedAmount { get; set; }
    public string? AppName { get; set; }
    public string? Number { get; set; }
    public string? RawMessage { get; set; }
}