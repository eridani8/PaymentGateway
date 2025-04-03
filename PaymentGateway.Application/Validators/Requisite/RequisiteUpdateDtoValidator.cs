using FluentValidation;
using PaymentGateway.Application.DTOs;
using PaymentGateway.Application.DTOs.Requisite;

namespace PaymentGateway.Application.Validators.Requisite;

public class RequisiteUpdateDtoValidator : AbstractValidator<RequisiteUpdateDto>
{
    public RequisiteUpdateDtoValidator()
    {
        Validators.ValidAmount(RuleFor(x => x.MaxAmount));
    }
}