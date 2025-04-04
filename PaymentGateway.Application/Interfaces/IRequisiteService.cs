using PaymentGateway.Application.DTOs;
using PaymentGateway.Application.DTOs.Requisite;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Application.Interfaces;

public interface IRequisiteService
{
    Task<RequisiteResponseDto> CreateRequisite(RequisiteCreateDto dto);
    Task<IEnumerable<RequisiteResponseDto>> GetAllRequisites();
    Task<RequisiteResponseDto?> GetRequisiteById(Guid id);
    Task<bool> UpdateRequisite(Guid id, RequisiteUpdateDto dto);
    Task<bool> DeleteRequisite(Guid id);
}