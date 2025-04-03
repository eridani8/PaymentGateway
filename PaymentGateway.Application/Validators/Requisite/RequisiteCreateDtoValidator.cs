using FluentValidation;
using PaymentGateway.Application.DTOs;
using PaymentGateway.Application.DTOs.Requisite;

namespace PaymentGateway.Application.Validators.Requisite;

// ReSharper disable once ClassNeverInstantiated.Global
public class RequisiteCreateDtoValidator : AbstractValidator<RequisiteCreateDto>
{
    public RequisiteCreateDtoValidator()
    {
        Validators.ValidFullName(RuleFor(x => x.FullName));
        Validators.ValidRequisiteType(RuleFor(x => x.Type));
        Validators.ValidPaymentData(RuleFor(x => x.PaymentData));
    }
}