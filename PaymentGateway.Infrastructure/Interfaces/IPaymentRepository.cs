using PaymentGateway.Core.Entities;

namespace PaymentGateway.Infrastructure.Interfaces;

public interface IPaymentRepository : IRepositoryBase<PaymentEntity>
{
    Task<List<PaymentEntity>> GetUnprocessedPayments();
    Task<List<PaymentEntity>> GetExpiredPayments();
    Task<PaymentEntity?> PaymentById(Guid id);
    Task<List<PaymentEntity>> GetAllPayments();
    Task<List<PaymentEntity>> GetUserPayments(Guid userId);
}