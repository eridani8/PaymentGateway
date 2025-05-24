using PaymentGateway.Shared.DTOs.Transaction;

namespace PaymentGateway.Shared.Validations.Validators.Transaction;

public class TransactionCreateDtoValidator : BaseValidator<TransactionCreateDto>
{
    public TransactionCreateDtoValidator()
    {
        RuleFor(x => x.PaymentId).ValidGuid();
        RuleFor(x => x.Source).ValidEnumValue();
        RuleFor(x => x.ExtractedAmount).ValidMoneyAmount();
    }
}