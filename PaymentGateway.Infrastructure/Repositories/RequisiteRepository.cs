using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentGateway.Core;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Infrastructure.Interfaces;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Infrastructure.Repositories;

public class RequisiteRepository(
    AppDbContext context,
    IOptions<GatewayConfig> gatewayConfig)
    : RepositoryBase<RequisiteEntity>(context), IRequisiteRepository
{
    public async Task<List<RequisiteEntity>> GetAllTracked()
    {
        return await GetSet()
            .Include(r => r.Payment)
            .Include(r => r.User)
            .ToListAsync();
    }

    public async Task<List<RequisiteEntity>> GetFreeRequisites()
    {
        var currentTime = DateTime.UtcNow;
        var currentTimeOnly = TimeOnly.FromDateTime(currentTime);

        var query = GetSet()
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

        var requisites = gatewayConfig.Value.AppointmentAlgorithm switch
        {
            RequisiteAssignmentAlgorithm.Priority => await query.OrderByDescending(r => r.Priority).ToListAsync(),
            RequisiteAssignmentAlgorithm.Distribution => await query.OrderBy(r => r.DayOperationsCount).ToListAsync(),
            _ => []
        };
        return requisites;
    }

    public async Task<int> GetUserRequisitesCount(Guid userId)
    {
        return await GetSet()
            .CountAsync(r => r.UserId == userId);
    }

    public async Task<List<RequisiteEntity>> GetAllRequisites()
    {
        var requisites = await GetSet()
            .Include(r => r.Payment)
            .Include(r => r.User)
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return requisites;
    }

    public async Task<List<RequisiteEntity>> GetUserRequisites(Guid userId)
    {
        var requisites = await GetSet()
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
        var requisite = await GetSet()
            .Include(r => r.Payment)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
        return requisite;
    }

    public async Task<RequisiteEntity?> HasSimilarRequisite(string paymentData)
    {
        return await GetSet()
            .FirstOrDefaultAsync(r =>
                r.PaymentData.Equals(paymentData));
    }

    public async Task<RequisiteEntity?> GetRequisiteByPaymentData(string paymentData)
    {
        return await GetSet()
            .Include(r => r.Payment)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r =>
                r.PaymentData == paymentData &&
                r.Payment != null &&
                r.Payment.Status == PaymentStatus.Pending);
    }
}