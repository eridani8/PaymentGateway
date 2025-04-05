using FluentValidation;
using PaymentGateway.Application.DTOs.Requisite;

namespace PaymentGateway.Application.Validators.Requisite;

public class RequisiteCreateDtoValidator : AbstractValidator<RequisiteCreateDto>
{
    public RequisiteCreateDtoValidator()
    {
        RuleFor(x => x.FullName).ValidFullName();
        When(x => ValidationRegexps.PhoneRegex().IsMatch(x.PaymentData), () =>
        {
            RuleFor(x => x.PaymentData).ValidPhoneNumber();
        }).Otherwise(() =>
        {
            RuleFor(x => x.PaymentData).ValidCreditCardNumber();
        });
        RuleFor(x => x.BankNumber).ValidBankAccount();
        RuleFor(x => x.MaxAmount ?? 0).ValidMoneyAmount();
        RuleFor(x => x.CooldownMinutes ?? 0).ValidCooldown();
        RuleFor(x => x.Priority ?? 0).ValidPriority();
    }
}