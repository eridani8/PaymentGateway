namespace PaymentGateway.Shared.DTOs.User;

public class CreateUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public bool IsActive { get; set; } = true;
    public int MaxRequisitesCount { get; set; }
    public decimal MaxDailyMoneyReceptionLimit { get; set; }
    public decimal ReceivedDailyFunds { get; set; }
    public DateTime LastFundsResetAt { get; set; }
}