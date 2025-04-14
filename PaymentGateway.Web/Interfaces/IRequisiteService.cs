using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Web.Services;

namespace PaymentGateway.Web.Interfaces;

public interface IRequisiteService
{
    Task<List<RequisiteDto>> GetRequisites();
    Task<List<RequisiteDto>> GetUserRequisites();
    Task<RequisiteDto?> CreateRequisite(RequisiteCreateDto dto);
    Task<Response> UpdateRequisite(Guid id, RequisiteUpdateDto dto);
    Task<RequisiteDto?> DeleteRequisite(Guid id);
} 