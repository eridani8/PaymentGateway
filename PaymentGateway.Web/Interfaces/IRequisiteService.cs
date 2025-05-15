using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.Types;
using PaymentGateway.Web.Services;

namespace PaymentGateway.Web.Interfaces;

public interface IRequisiteService
{
    Task<List<RequisiteDto>> GetRequisites();
    Task<List<RequisiteDto>> GetUserRequisites();
    Task<Response> CreateRequisite(RequisiteCreateDto dto);
    Task<Response> UpdateRequisite(Guid id, RequisiteUpdateDto dto);
    Task<Response> DeleteRequisite(Guid id);
    Task<List<RequisiteDto>> GetRequisitesByUserId(Guid userId);
} 