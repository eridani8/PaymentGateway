using FluentValidation;
using PaymentGateway.Application.DTOs.Transaction;

namespace PaymentGateway.Application.Validators.Transaction;

public class TransactionCreateDtoValidator : AbstractValidator<TransactionCreateDto>
{
    public TransactionCreateDtoValidator()
    {
        RuleFor(x => x.Source).ValidEnumValue();
        RuleFor(x => x.ExtractedAmount).ValidMoneyAmount();
        RuleFor(x => x.ReceivedAt).ValidDate();
    }
}