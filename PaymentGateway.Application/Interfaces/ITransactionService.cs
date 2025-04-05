using PaymentGateway.Application.DTOs.Transaction;

namespace PaymentGateway.Application.Interfaces;

public interface ITransactionService
{
    Task<TransactionResponseDto> CreateTransaction(TransactionCreateDto dto);
}