using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Shared.DTOs.Device;

public class DeviceDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; set; }
    public UserDto? User { get; set; }
    public required string DeviceName { get; init; }
    public required string Hw { get; set; }
}