using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Interfaces;

public interface IRequisiteRepository : IRepository<Requisite>
{
    Task<IEnumerable<Requisite>> GetActiveRequisites();
}