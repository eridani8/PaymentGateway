namespace PaymentGateway.Shared.DTOs.User;

public class TwoFactorStatusDto
{
    public bool IsEnabled { get; init; }
    public bool IsSetupRequired { get; init; }
}