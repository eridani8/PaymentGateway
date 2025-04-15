using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Shared.Validations.Validators.User;

public class CreateUserValidator : BaseValidator<CreateUserDto>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Username).ValidUsername();
        RuleFor(x => x.Password).ValidPassword();
        RuleFor(x => x.Roles).ValidRoles();
        RuleFor(x => x.MaxRequisitesCount).ValidNumber();
        RuleFor(x => x.MaxDailyMoneyReceptionLimit).ValidMoneyAmount();
    }
}