﻿using FluentValidation;
using PaymentGateway.Application.DTOs.Payment;

namespace PaymentGateway.Application.Validators.Payment;

public class PaymentCreateDtoValidator : AbstractValidator<PaymentCreateDto>
{
    public PaymentCreateDtoValidator()
    {
        RuleFor(x => x.PaymentId).ValidGuid();
        RuleFor(x => x.Amount).ValidMoneyAmount();
    }
}