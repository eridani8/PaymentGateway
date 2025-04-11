using FluentValidation;
using PaymentGateway.Shared.Models;

namespace PaymentGateway.Shared.Validations;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordModel>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword).ValidPassword();
        RuleFor(x => x.NewPassword).ValidPassword();
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Обязательное поле")
            .Equal(x => x.NewPassword).WithMessage("Пароли не совпадают");
    }
}