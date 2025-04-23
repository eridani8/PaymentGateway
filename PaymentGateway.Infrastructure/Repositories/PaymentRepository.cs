using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Shared.Enums;
using System.Text.Json;

namespace PaymentGateway.Infrastructure.Repositories;

public class PaymentRepository(
    AppDbContext context)
    : RepositoryBase<PaymentEntity>(context), IPaymentRepository
{

    public async Task<List<PaymentEntity>> GetUnprocessedPayments()
    {
        var payments = await
            Queryable()
                .Include(p => p.Requisite)
                .Where(p => p.Requisite == null && p.Status == PaymentStatus.Created)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        return payments;
    }

    public async Task<List<PaymentEntity>> GetExpiredPayments()
    {
        var now = DateTime.UtcNow;
        var payments = await
            Queryable()
                .Include(p => p.Requisite)
                .Where(p =>
                    p.ExpiresAt.HasValue &&
                    now >= p.ExpiresAt &&
                    p.Status != PaymentStatus.Confirmed &&
                    p.Status != PaymentStatus.ManualConfirm)
                .ToListAsync();
        return payments;
    }

    public async Task<PaymentEntity?> GetExistingPayment(Guid externalPaymentId)
    {
        var payment = await
            Queryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ExternalPaymentId == externalPaymentId);
        return payment;
    }

    public async Task<PaymentEntity?> PaymentById(Guid id)
    {
        var payment = await
            Queryable()
                .Include(p => p.Requisite)
                .ThenInclude(r => r.User)
                .Include(p => p.Transaction)
                .Include(p => p.ManualConfirmUser)
                .Include(p => p.CanceledByUser)
                .FirstOrDefaultAsync(p => p.Id == id);
        return payment;
    }

    public async Task<List<PaymentEntity>> GetAllPayments()
    {
        var payments = await
            Queryable()
                .Include(p => p.Requisite)
                .ThenInclude(p => p.User)
                .Include(p => p.Transaction)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        return payments;
    }

    public async Task<List<PaymentEntity>> GetUserPayments(Guid userId)
    {
        var payments = await
            Queryable()
                .Include(p => p.Requisite)
                .ThenInclude(p => p.User)
                .Include(p => p.Transaction)
                .Where(p => p.Requisite != null && p.Requisite.UserId == userId)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        return payments;
    }
}