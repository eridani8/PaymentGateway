using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Interfaces;

public interface IRequisiteRepository : IRepositoryBase<RequisiteEntity>
{
    Task<RequisiteEntity?> GetFreeRequisite();
    Task<IEnumerable<RequisiteEntity>> GetActiveRequisites();
}