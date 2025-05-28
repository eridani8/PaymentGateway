namespace PaymentGateway.Core.Configs;

public class GatewayConfig
{
    public required TimeSpan GatewayProcessDelay { get; init; }
} 