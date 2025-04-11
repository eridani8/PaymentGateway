using FluentValidation;
using PaymentGateway.Application.DTOs.Payment;
using PaymentGateway.Shared.Validations;

namespace PaymentGateway.Application.Validators.Payment;

public class PaymentCreateDtoValidator : AbstractValidator<PaymentCreateDto>
{
    public PaymentCreateDtoValidator()
    {
        RuleFor(x => x.ExternalPaymentId).ValidGuid();
        RuleFor(x => x.Amount).ValidMoneyAmount();
        RuleFor(x => x.UserId).ValidGuid();
    }
}