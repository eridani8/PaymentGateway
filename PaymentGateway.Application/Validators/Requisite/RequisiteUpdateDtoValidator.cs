using FluentValidation;
using PaymentGateway.Application.DTOs.Requisite;

namespace PaymentGateway.Application.Validators.Requisite;

public class RequisiteUpdateDtoValidator : AbstractValidator<RequisiteUpdateDto>
{
    public RequisiteUpdateDtoValidator()
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
        RuleFor(x => x.MaxAmount).ValidMoneyAmount();
        RuleFor(x => x.Cooldown).ValidCooldown();
        RuleFor(x => x.Priority).ValidPriority();
        When(x => x.WorkFrom != TimeOnly.MinValue || x.WorkTo != TimeOnly.MinValue, () =>
        {
            RuleFor(x => x.WorkFrom).ValidTime();
            RuleFor(x => x.WorkTo).ValidTime();
            RuleFor(x => x).ValidTimeRange(x => x.WorkFrom, x => x.WorkTo);
        });
    }
}