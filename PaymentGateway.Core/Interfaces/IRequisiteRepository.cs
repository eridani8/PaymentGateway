using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Interfaces;

public interface IRequisiteRepository : IRepositoryBase<RequisiteEntity>
{
    Task<List<RequisiteEntity>> GetFreeRequisites();
}