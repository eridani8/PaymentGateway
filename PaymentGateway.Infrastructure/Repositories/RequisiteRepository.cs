using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;

namespace PaymentGateway.Infrastructure.Repositories;

public class RequisiteRepository(AppDbContext context) : RepositoryBase<RequisiteEntity>(context), IRequisiteRepository
{
    public async Task<List<RequisiteEntity>> GetFreeRequisites()
    {
        return await
            GetAll()
                .Include(r => r.CurrentPayment)
                .Where(r => r.IsActive && r.CurrentPayment == null)
                .OrderByDescending(r => r.Priority)
                .ToListAsync();
    }
}