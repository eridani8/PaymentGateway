using PaymentGateway.Application.DTOs.Requisite;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Application.Interfaces;

public interface IRequisiteService
{
    void FreeRequisite(RequisiteEntity requisite, TransactionEntity transaction);
    void PendingRequisite(RequisiteEntity requisite, PaymentEntity payment);
    RequisiteEntity? SelectRequisite(List<RequisiteEntity> requisites, PaymentEntity payment);
    Task<RequisiteResponseDto> CreateRequisite(RequisiteCreateDto dto);
    Task<IEnumerable<RequisiteResponseDto>> GetAllRequisites();
    Task<RequisiteResponseDto?> GetRequisiteById(Guid id);
    Task<bool> UpdateRequisite(Guid id, RequisiteUpdateDto dto);
    Task<bool> DeleteRequisite(Guid id);
}