using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Shared.Validations.Validators.Requisite;

public class RequisiteUpdateDtoValidator : BaseValidator<RequisiteUpdateDto>
{
    public RequisiteUpdateDtoValidator()
    {
        RuleFor(x => x.FullName).ValidFullName();
        
        When(x => x.PaymentType == PaymentType.PhoneNumber, () =>
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
        When(x => x.DeviceId == Guid.Empty, () =>
        {
            RuleFor(x => x.DeviceId);
        });
    }
}