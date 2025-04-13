using PaymentGateway.Shared.DTOs.Transaction;

namespace PaymentGateway.Application.Interfaces;

public interface ITransactionService
{
    Task<TransactionDto> CreateTransaction(TransactionCreateDto dto);
}