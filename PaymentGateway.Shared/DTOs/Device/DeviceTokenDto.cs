namespace PaymentGateway.Shared.DTOs.Device;

public class DeviceTokenDto
{
    public string Token { get; init; } = string.Empty;
    public string QrCodeUri { get; init; } = string.Empty;
} 