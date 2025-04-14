namespace PaymentGateway.Shared.DTOs.User;

public class CreateUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public bool IsActive { get; set; } = true;
}