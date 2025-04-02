using FluentValidation;
using PaymentGateway.Application.DTOs;

namespace PaymentGateway.Application.Validators.Requisite;

public class RequisiteCreateDtoValidator : AbstractValidator<RequisiteCreateDto>
{
    public RequisiteCreateDtoValidator()
    {
        Validators.ValidFullName(RuleFor(x => x.FullName));
        Validators.ValidRequisiteType(RuleFor(x => x.Type));
        Validators.ValidPaymentData(RuleFor(x => x.PaymentData));
    }
}