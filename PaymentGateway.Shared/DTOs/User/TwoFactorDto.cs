namespace PaymentGateway.Shared.DTOs.User;

public class TwoFactorDto
{
    public string QrCodeUri { get; init; } = string.Empty;
    public string SharedKey { get; init; } = string.Empty;
}