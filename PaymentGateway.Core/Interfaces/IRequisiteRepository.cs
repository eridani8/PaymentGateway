using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Interfaces;

public interface IRequisiteRepository : IRepository<RequisiteEntity>
{
    Task<IEnumerable<RequisiteEntity>> GetActiveRequisites();
}