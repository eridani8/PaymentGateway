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
        var currentTime = DateTime.Now.TimeOfDay;
        return await
            GetAll()
                .Include(r => r.Payment)
                .Where(r => r.Status == RequisiteStatus.Active && r.Payment == null &&
                            ((r.WorkFrom == TimeSpan.Zero && r.WorkTo == TimeSpan.Zero) ||
                             (currentTime >= r.WorkFrom && currentTime <= r.WorkTo)))
                .OrderByDescending(r => r.Priority)
                .ToListAsync();
    }
}