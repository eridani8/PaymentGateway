﻿using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Shared.Enums;
using PaymentGateway.Shared.Interfaces;

namespace PaymentGateway.Infrastructure.Repositories;

public class PaymentRepository(AppDbContext context, ICache cache)
    : RepositoryBase<PaymentEntity>(context, cache), IPaymentRepository
{
    public async Task<List<PaymentEntity>> GetUnprocessedPayments()
    {
        return await
            QueryableGetAll()
                .Include(p => p.Requisite)
                .Where(p => p.Requisite == null && p.Status == PaymentStatus.Created)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
    }

    public async Task<List<PaymentEntity>> GetExpiredPayments()
    {
        var now = DateTime.UtcNow;
        return await
            QueryableGetAll()
                .Include(p => p.Requisite)
                .Where(p =>
                    p.ExpiresAt.HasValue && 
                    now >= p.ExpiresAt &&
                    p.Status != PaymentStatus.Confirmed &&
                    p.Status != PaymentStatus.ManualConfirm)
                .ToListAsync();
    }
}