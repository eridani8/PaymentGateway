using PaymentGateway.Shared.DTOs.Requisite;

namespace PaymentGateway.Web.Interfaces;

public interface IRequisiteService
{
    Task<List<RequisiteDto>> GetRequisites();
    Task<RequisiteDto?> CreateRequisite(RequisiteCreateDto dto);
    Task<bool> DeleteRequisite(Guid id);
} 