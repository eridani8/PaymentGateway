using FluentValidation;
using PaymentGateway.Application.DTOs.Requisite;

namespace PaymentGateway.Application.Validators.Requisite;

public class RequisiteCreateDtoValidator : AbstractValidator<RequisiteCreateDto>
{
    public RequisiteCreateDtoValidator()
    {
        RuleFor(x => x.FullName).ValidFullName();
        RuleFor(x => x.PhoneNumber).ValidPhoneNumber();
        RuleFor(x => x.CardNumber).ValidCreditCardNumber();
        RuleFor(x => x.BankAccountNumber).ValidBankAccount();
        RuleFor(x => x.MaxAmount ?? 0).ValidMoneyAmount();
        RuleFor(x => x.CooldownMinutes ?? 0).ValidCooldown();
        RuleFor(x => x.Priority ?? 0).ValidPriority();
    }
}