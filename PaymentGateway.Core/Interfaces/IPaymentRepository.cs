using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Interfaces;

public interface IPaymentRepository : IRepository<PaymentEntity>
{
    Task<PaymentEntity?> GetByPaymentById(Guid paymentId);
}