namespace PaymentGateway.Shared.DTOs.User;

public class DepositDto
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
}