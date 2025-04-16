using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Shared.Validations.Validators.User;

public class UpdateUserValidator : BaseValidator<UpdateUserDto>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Roles).ValidRoles();
        RuleFor(x => x.MaxRequisitesCount).ValidNumber();
        RuleFor(x => x.MaxDailyMoneyReceptionLimit).ValidMoneyAmount();
    }
} 