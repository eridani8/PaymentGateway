using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Enums;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;

namespace PaymentGateway.Infrastructure.Repositories;

public class RequisiteRepository(AppDbContext context, ICache cache)
    : RepositoryBase<RequisiteEntity>(context, cache), IRequisiteRepository
{
    public async Task<List<RequisiteEntity>> GetFreeRequisites()
    {
        var currentTime = DateTime.UtcNow;
        var currentTimeOnly = TimeOnly.FromDateTime(currentTime);
        return await
            QueryableGetAll()
                .Include(r => r.Payment)
                .Where(r => r.Status == RequisiteStatus.Active && r.Payment == null &&
                            ((r.WorkFrom == TimeOnly.MinValue && r.WorkTo == TimeOnly.MinValue) ||
                             (currentTimeOnly >= r.WorkFrom && currentTimeOnly <= r.WorkTo)))
                .OrderByDescending(r => r.Priority)
                .ToListAsync();
    }
}