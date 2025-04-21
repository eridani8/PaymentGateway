using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Shared.Validations.Validators.User;

public class TwoFactorVerifyDtoValidator : BaseValidator<TwoFactorVerifyDto>
{
    public TwoFactorVerifyDtoValidator()
    {
        RuleFor(x => x.Code).ValidAuthCode();
    }
}