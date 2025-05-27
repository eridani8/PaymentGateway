namespace PaymentGateway.Shared.DTOs.User;

public class WalletDto
{
    public Guid UserId { get; set; }
    public decimal Balance { get; set; }
    public decimal Frozen { get; set; }
    public decimal Profit { get; set; }
}