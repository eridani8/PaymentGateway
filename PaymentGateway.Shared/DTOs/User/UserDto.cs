namespace PaymentGateway.Shared.DTOs.User;

public class UserDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = [];
}