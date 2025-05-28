using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Core.Configs;

public class GatewaySettings
{
    public RequisiteAssignmentAlgorithm AppointmentAlgorithm { get; set; }
    public decimal UsdtExchangeRate { get; set; } = 1;
}