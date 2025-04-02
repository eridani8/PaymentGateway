using FluentValidation;
using PaymentGateway.Application.DTOs;

namespace PaymentGateway.Application.Validators.Requisite;

public class RequisiteValidator(IValidator<RequisiteCreateDto> createValidator, IValidator<RequisiteUpdateDto> updateValidator)
{
    public IValidator<RequisiteCreateDto> CreateValidator { get; } = createValidator;
    public IValidator<RequisiteUpdateDto> UpdateValidator { get; } = updateValidator;
}