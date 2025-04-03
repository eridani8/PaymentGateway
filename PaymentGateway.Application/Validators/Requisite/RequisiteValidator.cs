using FluentValidation;
using PaymentGateway.Application.DTOs;
using PaymentGateway.Application.DTOs.Requisite;
using PaymentGateway.Application.Interfaces;

namespace PaymentGateway.Application.Validators.Requisite;

public class RequisiteValidator(IValidator<RequisiteCreateDto> createValidator, IValidator<RequisiteUpdateDto> updateValidator) : IRequisiteValidator
{
    public IValidator<RequisiteCreateDto> CreateValidator { get; } = createValidator;
    public IValidator<RequisiteUpdateDto> UpdateValidator { get; } = updateValidator;
}