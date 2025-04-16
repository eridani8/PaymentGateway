using PaymentGateway.Shared.DTOs.Requisite;

namespace PaymentGateway.Shared.Validations.Validators.Requisite;

public class RequisiteCreateDtoValidator : BaseValidator<RequisiteCreateDto>
{
    public RequisiteCreateDtoValidator()
    {
        RuleFor(x => x.FullName).ValidFullName();
        RuleFor(x => x.PaymentType).ValidEnumValue();
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