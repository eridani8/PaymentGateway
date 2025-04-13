using PaymentGateway.Shared.DTOs.Payment;

namespace PaymentGateway.Shared.Validations.Validators.Payment;

public class PaymentCreateDtoValidator : BaseValidator<PaymentCreateDto>
{
    public PaymentCreateDtoValidator()
    {
        RuleFor(x => x.ExternalPaymentId).ValidGuid();
        RuleFor(x => x.Amount).ValidMoneyAmount();
        RuleFor(x => x.UserId).ValidGuid();
    }
}