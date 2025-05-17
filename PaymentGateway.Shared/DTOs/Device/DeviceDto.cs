namespace PaymentGateway.Shared.DTOs.Device;

public class DeviceDto
{
    public Guid Id { get; init; }
    public DateTime Timestamp { get; set; }
    public string? Model { get; init; }
    public DeviceAction Action { get; set; }
}