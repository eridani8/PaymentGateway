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
        
        var requisites = await
            QueryableGetAll()
                .Include(r => r.Payment)
                .Where(r => r.IsActive && r.Status == RequisiteStatus.Active && r.PaymentId == null &&
                            (
                                (r.WorkFrom == TimeOnly.MinValue && r.WorkTo == TimeOnly.MinValue) ||
                                (r.WorkFrom <= r.WorkTo && currentTimeOnly >= r.WorkFrom && currentTimeOnly <= r.WorkTo) ||
                                (r.WorkFrom > r.WorkTo && (currentTimeOnly >= r.WorkFrom || currentTimeOnly <= r.WorkTo))
                            ))
                .OrderByDescending(r => r.Priority)
                .ToListAsync();
        return requisites;
    }
}