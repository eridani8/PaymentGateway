using PaymentGateway.Application.DTOs.Requisite;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Application.Interfaces;

public interface IRequisiteService
{
    Task<RequisiteDto> CreateRequisite(RequisiteCreateDto dto);
    Task<IEnumerable<RequisiteDto>> GetAllRequisites();
    Task<RequisiteDto?> GetRequisiteById(Guid id);
    Task<bool> UpdateRequisite(Guid id, RequisiteUpdateDto dto);
    Task<bool> DeleteRequisite(Guid id);
}