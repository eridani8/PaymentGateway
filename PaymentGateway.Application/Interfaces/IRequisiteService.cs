using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Application.Results;

namespace PaymentGateway.Application.Interfaces;

public interface IRequisiteService
{
    Task<Result<RequisiteDto>> CreateRequisite(RequisiteCreateDto dto, Guid userId);
    Task<Result<IEnumerable<RequisiteDto>>> GetAllRequisites();
    Task<Result<IEnumerable<RequisiteDto>>> GetUserRequisites(Guid userId);
    Task<Result<RequisiteDto>> GetRequisiteById(Guid id);
    Task<Result<RequisiteDto>> UpdateRequisite(Guid id, RequisiteUpdateDto dto);
    Task<Result<RequisiteDto>> DeleteRequisite(Guid id);
}