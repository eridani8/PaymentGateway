using PaymentGateway.Core.Entities;

namespace PaymentGateway.Infrastructure.Interfaces;

public interface ITransactionRepository : IRepositoryBase<TransactionEntity>
{
    Task<List<TransactionEntity>> GetAllTransactions();
    Task<List<TransactionEntity>> GetUserTransactions(Guid userId);
}