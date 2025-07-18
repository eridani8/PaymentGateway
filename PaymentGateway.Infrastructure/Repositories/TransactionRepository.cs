﻿using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Infrastructure.Interfaces;

namespace PaymentGateway.Infrastructure.Repositories;

public class TransactionRepository(AppDbContext context)
    : RepositoryBase<TransactionEntity>(context), ITransactionRepository
{
    public async Task<List<TransactionEntity>> GetAllTransactions()
    {
        return await GetSet()
            .Include(t => t.Requisite)
            .Include(t => t.Payment)
            .OrderByDescending(t => t.ReceivedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<TransactionEntity>> GetUserTransactions(Guid userId)
    {
        return await GetSet()
            .Include(t => t.Requisite)
            .Include(t => t.Payment)
            .Where(t => t.Requisite != null && t.Requisite.UserId == userId)
            .OrderByDescending(t => t.ReceivedAt)
            .AsNoTracking()
            .ToListAsync();
    }
}