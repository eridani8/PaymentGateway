using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Core;

public class GatewaySettings
{
    public RequisiteAssignmentAlgorithm AppointmentAlgorithm { get; set; } = RequisiteAssignmentAlgorithm.Priority;
} 