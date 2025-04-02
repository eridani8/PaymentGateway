using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Interfaces;

public interface IPaymentRepository : IRepositoryBase<PaymentEntity>
{
    Task<PaymentEntity?> GetByPaymentById(Guid paymentId);
}