using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;

namespace PaymentGateway.Infrastructure.Repositories;

public class PaymentRepository(AppDbContext context, ICache cache) : RepositoryBase<PaymentEntity>(context, cache), IPaymentRepository
{
    public async Task<List<PaymentEntity>> GetUnprocessedPayments()
    {
        return await
            GetAll()
                .Include(p => p.Requisite)
                .Where(p => p.Requisite == null)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
    }

    public async Task<List<PaymentEntity>> GetExpiredPayments()
    {
        return await
            GetAll()
                .Include(p => p.Requisite)
                .Where(p => DateTime.UtcNow >= p.ExpiresAt)
                .ToListAsync();
    }
    
    public void DeletePayments(IEnumerable<PaymentEntity> entities)
    {
        foreach (var entity in entities)
        {
            Delete(entity);
        }
    }
}