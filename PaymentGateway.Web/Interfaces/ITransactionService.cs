using PaymentGateway.Shared.DTOs.Transaction;

namespace PaymentGateway.Web.Interfaces;

public interface ITransactionService
{
    Task<List<TransactionDto>> GetTransactions();
    Task<List<TransactionDto>> GetUserTransactions();
} 