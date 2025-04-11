using FluentValidation;
using PaymentGateway.Shared.Models;

namespace PaymentGateway.Shared.Validations;

public class LoginModelValidator : AbstractValidator<LoginModel>
{
    public LoginModelValidator()
    {
        RuleFor(x => x.Username).ValidUsername();
        RuleFor(x => x.Password).ValidPassword();
    }
}