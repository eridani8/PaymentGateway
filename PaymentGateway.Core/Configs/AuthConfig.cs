namespace PaymentGateway.Core.Configs;

public class AuthConfig
{
    public required string SecretKey { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required TimeSpan Expiration { get; init; }
}