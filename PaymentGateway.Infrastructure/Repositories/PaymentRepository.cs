using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Shared.DTOs.Payment;
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

    public async Task<PaymentEntity?> GetExistingPayment(Guid externalPaymentId)
    {
        return await
            QueryableGetAll()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ExternalPaymentId == externalPaymentId);
    }

    public async Task<PaymentEntity?> PaymentById(Guid id)
    {
        return await
            QueryableGetAll()
                .Include(p => p.Requisite)
                .ThenInclude(r => r.User)
                .Include(p => p.Transaction)
                .Include(p => p.ManualConfirmUser)
                .Include(p => p.CanceledByUser)
                .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<PaymentEntity>> GetAllPayments()
    {
        return await
            QueryableGetAll()
                .Include(p => p.Requisite)
                .ThenInclude(p => p.User)
                .Include(p => p.Transaction)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
    }

    public async Task<List<PaymentEntity>> GetUserPayments(Guid userId)
    {
        return await
            QueryableGetAll()
                .Include(p => p.Requisite)
                .ThenInclude(p => p.User)
                .Include(p => p.Transaction)
                .Where(p => p.Requisite != null && p.Requisite.UserId == userId)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
    }
}