using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Interfaces;

public interface IRequisiteRepository : IRepositoryBase<RequisiteEntity>
{
    Task<List<RequisiteEntity>> GetFreeRequisites();
    Task<int> GetUserRequisitesCount(Guid userId);
    Task<List<RequisiteEntity>> GetAllRequisites();
    Task<List<RequisiteEntity>> GetUserRequisites(Guid userId);
    Task<RequisiteEntity?> GetRequisiteById(Guid id);
    Task<RequisiteEntity?> HasSimilarRequisite(string paymentData);
}