using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Core;

public class GatewaySettings
{
    public PaymentAssignmentAlgorithm AppointmentAlgorithm { get; set; } = PaymentAssignmentAlgorithm.Priority;
} 