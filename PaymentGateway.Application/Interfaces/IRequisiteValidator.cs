using FluentValidation;
using PaymentGateway.Application.DTOs.Requisite;

namespace PaymentGateway.Application.Interfaces;

public interface IRequisiteValidator
{
    IValidator<RequisiteCreateDto> CreateValidator { get; }
    IValidator<RequisiteUpdateDto> UpdateValidator { get; }
}