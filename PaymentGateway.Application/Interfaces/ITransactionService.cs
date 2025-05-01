using PaymentGateway.Application.Results;
using PaymentGateway.Shared.DTOs.Transaction;

namespace PaymentGateway.Application.Interfaces;

public interface ITransactionService
{
    Task<Result<TransactionDto>> CreateTransaction(TransactionCreateDto dto);
    Task<Result<List<TransactionDto>>> GetAllTransactions();
    Task<Result<List<TransactionDto>>> GetUserTransactions(Guid userId);
}