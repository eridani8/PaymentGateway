namespace PaymentGateway.Shared.DTOs.Device;

public class DeviceDto
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Model { get; set; }
}