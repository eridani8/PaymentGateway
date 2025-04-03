using FluentValidation;
using PaymentGateway.Application.DTOs;
using PaymentGateway.Application.DTOs.Requisite;

namespace PaymentGateway.Application.Validators.Requisite;

public class RequisiteUpdateDtoValidator : AbstractValidator<RequisiteUpdateDto>
{
    public RequisiteUpdateDtoValidator()
    {
        Validators.ValidEnumValue(RuleFor(x => x.Type));
        Validators.ValidPaymentData(RuleFor(x => x.PaymentData));
        Validators.ValidFullName(RuleFor(x => x.FullName));
        Validators.ValidAmount(RuleFor(x => x.MaxAmount ?? 0));
        Validators.ValidAmount(RuleFor(x => x.CooldownMinutes ?? 0));
        Validators.ValidAmount(RuleFor(x => x.Priority ?? 0));
    }
}