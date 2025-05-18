namespace PaymentGateway.Shared.Types;

public class WebSocketSettings
{
    public required string BaseAddress { get; init; }
    public required string HubName { get; init; }
}