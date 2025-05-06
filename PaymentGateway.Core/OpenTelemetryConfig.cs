namespace PaymentGateway.Core;

public class OpenTelemetryConfig
{
    public required string Endpoint { get; init; }
    public required string Token { get; init; }
}