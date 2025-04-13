using PaymentGateway.Core.Entities;
using PaymentGateway.Shared.DTOs.Requisite;

namespace PaymentGateway.Application.Interfaces;

public interface IRequisiteService
{
    Task<RequisiteDto> CreateRequisite(RequisiteCreateDto dto);
    Task<IEnumerable<RequisiteDto>> GetAllRequisites();
    Task<RequisiteDto?> GetRequisiteById(Guid id);
    Task<bool> UpdateRequisite(Guid id, RequisiteUpdateDto dto);
    Task<bool> DeleteRequisite(Guid id);
}