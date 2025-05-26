using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Shared.Validations.Validators.User;

public class DepositValidator : BaseValidator<DepositDto>
{
    public DepositValidator()
    {
        RuleFor(x => x.UserId).ValidGuid();
        RuleFor(x => x.Amount).ValidMoneyAmount();
    }
}