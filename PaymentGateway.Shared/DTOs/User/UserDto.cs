namespace PaymentGateway.Shared.DTOs.User;

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int RequisitesCount { get; set; }
    public int MaxRequisitesCount { get; set; }
    public decimal MaxDailyMoneyReceptionLimit { get; set; }
    public decimal ReceivedDailyFunds { get; set; }
    public DateTime LastFundsResetAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal Balance { get; set; }
    public decimal Frozen { get; set; }
    public decimal Profit { get; set; }
}