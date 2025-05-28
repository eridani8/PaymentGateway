using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Infrastructure.Interfaces;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Infrastructure.Repositories;

public class PaymentRepository(
    AppDbContext context)
    : RepositoryBase<PaymentEntity>(context), IPaymentRepository
{
    public async Task<List<PaymentEntity>> GetUnprocessedPayments()
    {
        return await GetSet()
            .Include(p => p.Requisite)
            .Include(p => p.User)
            .Where(p => p.Requisite == null && p.Status == PaymentStatus.Created)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<PaymentEntity>> GetExpiredPayments()
    {
        var now = DateTime.UtcNow;
        return await GetSet()
            .Include(p => p.Requisite)
            .Include(p => p.User)
            .Where(p =>
                p.ExpiresAt.HasValue &&
                now >= p.ExpiresAt &&
                p.Status != PaymentStatus.Confirmed &&
                p.Status != PaymentStatus.ManualConfirm)
            .ToListAsync();
    }

    public async Task<PaymentEntity?> PaymentById(Guid id)
    {
        return await GetSet()
            .Include(p => p.Requisite)
            .ThenInclude(r => r.User)
            .Include(p => p.User)
            .Include(p => p.Transaction)
            .Include(p => p.ManualConfirmUser)
            .Include(p => p.CanceledByUser)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<PaymentEntity>> GetAllPayments()
    {
        return await GetSet()
            .Include(p => p.Requisite)
            .ThenInclude(p => p.User)
            .Include(p => p.Transaction)
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<PaymentEntity>> GetUserPayments(Guid userId)
    {
        return await GetSet()
            .Include(p => p.Requisite)
            .ThenInclude(p => p.User)
            .Include(p => p.Transaction)
            .Where(p => p.Requisite != null && p.Requisite.UserId == userId)
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}