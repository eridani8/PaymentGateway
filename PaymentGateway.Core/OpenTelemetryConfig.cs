namespace PaymentGateway.Core;

public class OpenTelemetryConfig
{
    public required string ServiceName { get; set; }
    public required string ServiceVersion { get; set; }
    public required string Endpoint { get; set; }
}