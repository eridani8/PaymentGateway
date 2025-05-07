using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Core.Configs;

public class GatewayConfig
{
    public required RequisiteAssignmentAlgorithm AppointmentAlgorithm { get; set; } = RequisiteAssignmentAlgorithm.Priority;
    public required TimeSpan GatewayProcessDelay { get; init; }
} 