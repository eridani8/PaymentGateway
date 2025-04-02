using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Interfaces;

public interface IRequisiteRepository : IRepositoryBase<RequisiteEntity>
{
    Task<IEnumerable<RequisiteEntity>> GetActiveRequisites();
}