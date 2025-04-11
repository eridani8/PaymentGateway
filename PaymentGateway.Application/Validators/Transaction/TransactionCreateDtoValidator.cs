using FluentValidation;
using PaymentGateway.Application.DTOs.Transaction;
using PaymentGateway.Shared.Validations;

namespace PaymentGateway.Application.Validators.Transaction;

public class TransactionCreateDtoValidator : AbstractValidator<TransactionCreateDto>
{
    public TransactionCreateDtoValidator()
    {
        When(x => ValidationRegexps.PhoneRegex().IsMatch(x.PaymentData), () =>
        {
            RuleFor(x => x.PaymentData).ValidPhoneNumber();
        }).Otherwise(() =>
        {
            RuleFor(x => x.PaymentData).ValidCreditCardNumber();
        });
        RuleFor(x => x.Source).ValidEnumValue();
        RuleFor(x => x.ExtractedAmount).ValidMoneyAmount();
        RuleFor(x => x.ReceivedAt).ValidDate();
    }
}