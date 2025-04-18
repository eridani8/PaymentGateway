using PaymentGateway.Shared.DTOs.Transaction;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Web.Interfaces;

public interface ITransactionService
{
    Task<List<TransactionDto>> GetTransactions();
    Task<List<TransactionDto>> GetUserTransactions();
    Task<TransactionDto?> SimulatePayment(string paymentData, decimal amount, TransactionSource source);
} 