using FluentValidation;
using PaymentGateway.Shared.DTOs;
using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Shared.Validations;

public class LoginModelValidator : AbstractValidator<LoginDto>
{
    public LoginModelValidator()
    {
        RuleFor(x => x.Username).ValidUsername();
        RuleFor(x => x.Password).ValidPassword();
    }
}