using PaymentGateway.Shared.DTOs.Payment;

namespace PaymentGateway.Shared.Validations.Validators.Payment;

public class PaymentCancelDtoValidator : BaseValidator<PaymentCancelDto>
{
    public PaymentCancelDtoValidator()
    {
        RuleFor(x => x.PaymentId).ValidGuid();
    }
} 