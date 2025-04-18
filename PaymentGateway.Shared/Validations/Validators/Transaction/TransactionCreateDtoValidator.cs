﻿using PaymentGateway.Shared.DTOs.Transaction;

namespace PaymentGateway.Shared.Validations.Validators.Transaction;

public class TransactionCreateDtoValidator : BaseValidator<TransactionCreateDto>
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
    }
}