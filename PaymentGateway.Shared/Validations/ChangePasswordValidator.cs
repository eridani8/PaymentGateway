using FluentValidation;
using PaymentGateway.Shared.DTOs;
using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Shared.Validations;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
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