using PaymentGateway.Shared.DTOs.Payment;

namespace PaymentGateway.Shared.Validations.Validators.Payment;

public class PaymentManualConfirmDtoValidator : BaseValidator<PaymentManualConfirmDto>
{
    public PaymentManualConfirmDtoValidator()
    {
        RuleFor(x => x.PaymentId).ValidGuid();
    }
}