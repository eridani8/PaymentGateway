﻿using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Shared.Enums;

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
                             r.User.ReceivedDailyFunds < r.User.MaxDailyMoneyReceptionLimit))
                .OrderByDescending(r => r.Priority)
                .ThenBy(r => r.LastOperationTime ?? DateTime.MaxValue)
                .ToListAsync();
        return requisites;
    }
}