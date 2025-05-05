using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Core;

public class GatewayConfig
{
    public RequisiteAssignmentAlgorithm AppointmentAlgorithm { get; set; } = RequisiteAssignmentAlgorithm.Priority;
} 