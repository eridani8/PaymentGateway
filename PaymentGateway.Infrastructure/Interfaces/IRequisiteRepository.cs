using PaymentGateway.Core.Entities;

namespace PaymentGateway.Infrastructure.Interfaces;

public interface IRequisiteRepository : IRepositoryBase<RequisiteEntity>
{
    Task<List<RequisiteEntity>> GetAll();
    Task<List<RequisiteEntity>> GetActiveRequisites();
    Task<int> GetUserRequisitesCount(Guid userId);
    Task<List<RequisiteEntity>> GetAllRequisites();
    Task<List<RequisiteEntity>> GetUserRequisites(Guid userId);
    Task<RequisiteEntity?> GetRequisiteById(Guid id);
    Task<RequisiteEntity?> HasSimilarRequisite(string paymentData);
    Task<RequisiteEntity?> GetRequisiteByPaymentData(string paymentData);
}