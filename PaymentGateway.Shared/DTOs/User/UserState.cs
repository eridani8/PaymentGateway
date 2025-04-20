namespace PaymentGateway.Shared.DTOs.User;

public class UserState
{
    public Guid Id { get; set; }
    public required string Username { get; set; }
    public List<string> Roles { get; set; } = [];
}