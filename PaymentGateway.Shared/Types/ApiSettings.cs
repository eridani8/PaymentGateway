namespace PaymentGateway.Shared.Types;

public class ApiSettings
{
    public required string BaseAddress { get; init; }
    public required string HubName { get; init; }
}