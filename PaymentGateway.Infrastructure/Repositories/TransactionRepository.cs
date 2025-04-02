using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;

namespace PaymentGateway.Infrastructure.Repositories;

public class TransactionRepository(AppDbContext context) : RepositoryBase<TransactionEntity>(context), ITransactionRepository 
{
    
}