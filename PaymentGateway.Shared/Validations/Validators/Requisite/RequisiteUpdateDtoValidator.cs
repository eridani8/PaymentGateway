using PaymentGateway.Shared.DTOs.Requisite;

namespace PaymentGateway.Shared.Validations.Validators.Requisite;

public class RequisiteUpdateDtoValidator : BaseValidator<RequisiteUpdateDto>
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
    }
}