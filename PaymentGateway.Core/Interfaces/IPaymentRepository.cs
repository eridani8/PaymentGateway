using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Interfaces;

public interface IPaymentRepository : IRepositoryBase<PaymentEntity>
{
    Task<List<PaymentEntity>> GetUnprocessedPayments();
    Task<List<PaymentEntity>> GetExpiredPayments();
    Task<PaymentEntity?> GetExistingPayment(Guid externalPaymentId);
    Task<PaymentEntity?> PaymentById(Guid id);
    Task<List<PaymentEntity>> GetAllPayments();
    Task<List<PaymentEntity>> GetUserPayments(Guid userId);
}