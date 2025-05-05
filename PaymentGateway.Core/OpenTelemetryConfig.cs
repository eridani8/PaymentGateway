namespace PaymentGateway.Core;

public class OpenTelemetryConfig
{
    public required string ServiceName { get; init; }
    public required string Endpoint { get; init; }
}