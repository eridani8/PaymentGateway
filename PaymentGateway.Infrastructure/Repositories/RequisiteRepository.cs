using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentGateway.Core;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Infrastructure.Repositories;

public class RequisiteRepository(
    AppDbContext context,
    IOptions<GatewaySettings> gatewaySettings)
    : RepositoryBase<RequisiteEntity>(context), IRequisiteRepository
{
    public async Task<List<RequisiteEntity>> GetAll()
    {
        return await Queryable().ToListAsync();
    }

    public async Task<List<RequisiteEntity>> GetFreeRequisites()
    {
        var currentTime = DateTime.UtcNow;
        var currentTimeOnly = TimeOnly.FromDateTime(currentTime);

        var query = Queryable()
            .Include(r => r.Payment)
            .Include(r => r.User)
            .Where(r => r.IsActive && r.Status == RequisiteStatus.Active && r.PaymentId == null &&
                        (
                            (r.WorkFrom == TimeOnly.MinValue && r.WorkTo == TimeOnly.MinValue) ||
                            (r.WorkFrom <= r.WorkTo && currentTimeOnly >= r.WorkFrom &&
                             currentTimeOnly <= r.WorkTo) ||
                            (r.WorkFrom > r.WorkTo &&
                             (currentTimeOnly >= r.WorkFrom || currentTimeOnly <= r.WorkTo))
                        ) &&
                        (r.User.MaxDailyMoneyReceptionLimit == 0 ||
                         r.User.ReceivedDailyFunds < r.User.MaxDailyMoneyReceptionLimit));

        var requisites = gatewaySettings.Value.AppointmentAlgorithm switch
        {
            RequisiteAssignmentAlgorithm.Priority => await query.OrderByDescending(r => r.Priority).ToListAsync(),
            RequisiteAssignmentAlgorithm.Distribution => await query.OrderBy(r => r.DayOperationsCount).ToListAsync(),
            _ => []
        };
        return requisites;
    }

    public async Task<int> GetUserRequisitesCount(Guid userId)
    {
        return await Queryable()
            .CountAsync(r => r.UserId == userId);
    }

    public async Task<List<RequisiteEntity>> GetAllRequisites()
    {
        var requisites = await Queryable()
            .Include(r => r.Payment)
            .Include(r => r.User)
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return requisites;
    }

    public async Task<List<RequisiteEntity>> GetUserRequisites(Guid userId)
    {
        var requisites = await Queryable()
            .Include(r => r.Payment)
            .Include(r => r.User)
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return requisites;
    }

    public async Task<RequisiteEntity?> GetRequisiteById(Guid id)
    {
        var requisite = await Queryable()
            .Include(r => r.Payment)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
        return requisite;
    }

    public async Task<RequisiteEntity?> HasSimilarRequisite(string paymentData)
    {
        return await Queryable()
            .FirstOrDefaultAsync(r =>
                r.PaymentData.Equals(paymentData));
    }
}