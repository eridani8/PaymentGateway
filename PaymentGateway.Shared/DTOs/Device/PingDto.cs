namespace PaymentGateway.Shared.DTOs.Device;

public class PingDto
{
    public Guid Id { get; init; }
    public string? Model { get; set; }
}