using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Interfaces;

public interface IPaymentRepository : IRepositoryBase<PaymentEntity>
{
    Task<List<PaymentEntity>> GetUnprocessedPayments();
    Task<List<PaymentEntity>> GetExpiredPayments();
    Task DeletePayments(IEnumerable<PaymentEntity> entities);
}