namespace PaymentGateway.Application.Types;

public class DeviceState
{
    public DateTime Timestamp { get; set; }
    public required string Model { get; set; }
}