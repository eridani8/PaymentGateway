using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;

namespace PaymentGateway.Infrastructure.Repositories;

public class TransactionRepository(AppDbContext context)
    : RepositoryBase<TransactionEntity>(context), ITransactionRepository
{
    public async Task<List<TransactionEntity>> GetAllTransactions()
    {
        return await
            Queryable()
                .OrderByDescending(t => t.ReceivedAt)
                .AsNoTracking()
                .ToListAsync();
    }

    public async Task<List<TransactionEntity>> GetUserTransactions(Guid userId)
    {
        return await
            Queryable()
                .Where(t => t.Requisite != null && t.Requisite.UserId == userId)
                .OrderByDescending(t => t.ReceivedAt)
                .AsNoTracking()
                .ToListAsync();
    }
}