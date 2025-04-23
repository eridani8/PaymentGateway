using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Interfaces;

public interface ITransactionRepository : IRepositoryBase<TransactionEntity>
{
    Task<List<TransactionEntity>> GetAllTransactions();
    Task<List<TransactionEntity>> GetUserTransactions(Guid userId);
}