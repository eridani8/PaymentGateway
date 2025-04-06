using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Enums;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;

namespace PaymentGateway.Infrastructure.Repositories;

public class RequisiteRepository(AppDbContext context, ICache cache) : RepositoryBase<RequisiteEntity>(context, cache), IRequisiteRepository
{
    public async Task<List<RequisiteEntity>> GetFreeRequisites()
    {
        return await
            GetAll()
                .Include(r => r.CurrentPayment)
                .Where(r => r.Status == RequisiteStatus.Active && r.CurrentPayment == null)
                .OrderByDescending(r => r.Priority)
                .ToListAsync();
    }
}