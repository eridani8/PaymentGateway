namespace PaymentGateway.Shared.DTOs.Device;

public class DeviceDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; set; }
    public string? DeviceData { get; init; }
}