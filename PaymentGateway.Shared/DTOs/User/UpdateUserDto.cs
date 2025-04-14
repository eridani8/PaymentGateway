namespace PaymentGateway.Shared.DTOs.User;

public class UpdateUserDto
{
    public Guid Id { get; init; }
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = [];
} 