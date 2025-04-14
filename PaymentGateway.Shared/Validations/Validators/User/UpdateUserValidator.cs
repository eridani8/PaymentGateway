using FluentValidation;
using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Shared.Validations.Validators.User;

public class UpdateUserValidator : BaseValidator<UpdateUserDto>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Id).ValidGuid();
        RuleFor(x => x.Roles).ValidRoles();
    }
} 