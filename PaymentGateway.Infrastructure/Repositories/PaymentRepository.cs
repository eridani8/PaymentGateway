using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Shared.Enums;
using System.Text.Json;

namespace PaymentGateway.Infrastructure.Repositories;

public class PaymentRepository(
    AppDbContext context, 
    ICache cache,
    ILogger<PaymentRepository> logger,
    JsonSerializerOptions options)
    : RepositoryBase<PaymentEntity>(context), IPaymentRepository
{
    // private const string AllPaymentsCacheKey = "Payments:All";
    // private const string UserPaymentsCacheKeyPrefix = "Payments:User:";
    // private const string PaymentByIdCacheKeyPrefix = "Payment:";
    // private const string UnprocessedPaymentsCacheKey = "Payments:Unprocessed";
    // private const string ExpiredPaymentsCacheKey = "Payments:Expired";
    // private const string ExternalPaymentCacheKeyPrefix = "Payment:External:";
    // private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(5);

    public async Task<List<PaymentEntity>> GetUnprocessedPayments()
    {
        // var cachedPayments = cache.GetString(UnprocessedPaymentsCacheKey);
        // if (!string.IsNullOrEmpty(cachedPayments))
        // {
        //     try
        //     {
        //         return JsonSerializer.Deserialize<List<PaymentEntity>>(cachedPayments, options) ?? [];
        //     }
        //     catch (Exception ex)
        //     {
        //         logger.LogWarning(ex, "Ошибка при десериализации кешированных необработанных платежей");
        //         cache.Remove(UnprocessedPaymentsCacheKey);
        //     }
        // }

        var payments = await
            Queryable()
                .Include(p => p.Requisite)
                .Where(p => p.Requisite == null && p.Status == PaymentStatus.Created)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        return payments;
        // try
        // {
        //     var jsonPayments = JsonSerializer.Serialize(payments, options);
        //     cache.SetString(UnprocessedPaymentsCacheKey, jsonPayments, TimeSpan.FromSeconds(30));
        // }
        // catch (Exception ex)
        // {
        //     logger.LogError(ex, "Ошибка при кешировании необработанных платежей");
        // }
        //
        // return payments;
    }

    public async Task<List<PaymentEntity>> GetExpiredPayments()
    {
        // var cachedPayments = cache.GetString(ExpiredPaymentsCacheKey);
        // if (!string.IsNullOrEmpty(cachedPayments))
        // {
        //     try
        //     {
        //         return JsonSerializer.Deserialize<List<PaymentEntity>>(cachedPayments, options) ?? [];
        //     }
        //     catch (Exception ex)
        //     {
        //         logger.LogWarning(ex, "Ошибка при десериализации кешированных истекших платежей");
        //         cache.Remove(ExpiredPaymentsCacheKey);
        //     }
        // }

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
        // try
        // {
        //     var jsonPayments = JsonSerializer.Serialize(payments, options);
        //     cache.SetString(ExpiredPaymentsCacheKey, jsonPayments, TimeSpan.FromSeconds(30));
        // }
        // catch (Exception ex)
        // {
        //     logger.LogError(ex, "Ошибка при кешировании истекших платежей");
        // }
        //
        // return payments;
    }

    public async Task<PaymentEntity?> GetExistingPayment(Guid externalPaymentId)
    {
        // var cacheKey = $"{ExternalPaymentCacheKeyPrefix}{externalPaymentId}";
        // var cachedPayment = cache.GetString(cacheKey);
        //
        // if (!string.IsNullOrEmpty(cachedPayment))
        // {
        //     try
        //     {
        //         return JsonSerializer.Deserialize<PaymentEntity>(cachedPayment, options);
        //     }
        //     catch (Exception ex)
        //     {
        //         logger.LogWarning(ex, "Ошибка при десериализации кешированного платежа по внешнему ID");
        //         cache.Remove(cacheKey);
        //     }
        // }

        var payment = await
            Queryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ExternalPaymentId == externalPaymentId);
        return payment;
        // if (payment != null)
        // {
        //     try
        //     {
        //         var jsonPayment = JsonSerializer.Serialize(payment, options);
        //         cache.SetString(cacheKey, jsonPayment, DefaultCacheDuration);
        //     }
        //     catch (Exception ex)
        //     {
        //         logger.LogError(ex, "Ошибка при кешировании платежа по внешнему ID");
        //     }
        // }
        //
        // return payment;
    }

    public async Task<PaymentEntity?> PaymentById(Guid id)
    {
        // var cacheKey = $"{PaymentByIdCacheKeyPrefix}{id}";
        // var cachedPayment = cache.GetString(cacheKey);
        //
        // if (!string.IsNullOrEmpty(cachedPayment))
        // {
        //     try
        //     {
        //         return JsonSerializer.Deserialize<PaymentEntity>(cachedPayment, options);
        //     }
        //     catch (Exception ex)
        //     {
        //         logger.LogWarning(ex, "Ошибка при десериализации кешированного платежа");
        //         cache.Remove(cacheKey);
        //     }
        // }

        var payment = await
            Queryable()
                .Include(p => p.Requisite)
                .ThenInclude(r => r.User)
                .Include(p => p.Transaction)
                .Include(p => p.ManualConfirmUser)
                .Include(p => p.CanceledByUser)
                .FirstOrDefaultAsync(p => p.Id == id);
        return payment;
        // if (payment != null)
        // {
        //     try
        //     {
        //         var jsonPayment = JsonSerializer.Serialize(payment, options);
        //         cache.SetString(cacheKey, jsonPayment, DefaultCacheDuration);
        //     }
        //     catch (Exception ex)
        //     {
        //         logger.LogError(ex, "Ошибка при кешировании платежа по ID");
        //     }
        // }
        //
        // return payment;
    }

    public async Task<List<PaymentEntity>> GetAllPayments()
    {
        // var cachedPayments = cache.GetString(AllPaymentsCacheKey);
        // if (!string.IsNullOrEmpty(cachedPayments))
        // {
        //     try
        //     {
        //         return JsonSerializer.Deserialize<List<PaymentEntity>>(cachedPayments, options) ?? [];
        //     }
        //     catch (Exception ex)
        //     {
        //         logger.LogWarning(ex, "Ошибка при десериализации кешированных платежей");
        //         cache.Remove(AllPaymentsCacheKey);
        //     }
        // }

        var payments = await
            Queryable()
                .Include(p => p.Requisite)
                .ThenInclude(p => p.User)
                .Include(p => p.Transaction)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        return payments;
        // try
        // {
        //     var jsonPayments = JsonSerializer.Serialize(payments, options);
        //     cache.SetString(AllPaymentsCacheKey, jsonPayments, DefaultCacheDuration);
        // }
        // catch (Exception ex)
        // {
        //     logger.LogError(ex, "Ошибка при кешировании всех платежей");
        // }
        //
        // return payments;
    }

    public async Task<List<PaymentEntity>> GetUserPayments(Guid userId)
    {
        // var cacheKey = $"{UserPaymentsCacheKeyPrefix}{userId}";
        // var cachedPayments = cache.GetString(cacheKey);
        //
        // if (!string.IsNullOrEmpty(cachedPayments))
        // {
        //     try
        //     {
        //         return JsonSerializer.Deserialize<List<PaymentEntity>>(cachedPayments, options) ?? [];
        //     }
        //     catch (Exception ex)
        //     {
        //         logger.LogWarning(ex, "Ошибка при десериализации кешированных платежей пользователя");
        //         cache.Remove(cacheKey);
        //     }
        // }

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
        // try
        // {
        //     var jsonPayments = JsonSerializer.Serialize(payments, options);
        //     cache.SetString(cacheKey, jsonPayments, DefaultCacheDuration);
        // }
        // catch (Exception ex)
        // {
        //     logger.LogError(ex, "Ошибка при кешировании платежей пользователя");
        // }
        //
        // return payments;
    }

    // public void InvalidateCache(PaymentEntity payment)
    // {
    //     try
    //     {
    //         cache.Remove($"{PaymentByIdCacheKeyPrefix}{payment.Id}");
    //         if (payment.ExternalPaymentId != Guid.Empty)
    //         {
    //             cache.Remove($"{ExternalPaymentCacheKeyPrefix}{payment.ExternalPaymentId}");
    //         }
    //         
    //         cache.Remove(AllPaymentsCacheKey);
    //         cache.Remove(UnprocessedPaymentsCacheKey);
    //         cache.Remove(ExpiredPaymentsCacheKey);
    //         
    //         if (payment.Requisite?.UserId != null)
    //         {
    //             cache.Remove($"{UserPaymentsCacheKeyPrefix}{payment.Requisite.UserId}");
    //         }
    //         
    //         logger.LogDebug("Кеш для платежа {paymentId} был инвалидирован", payment.Id);
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, "Ошибка при инвалидации кеша для платежа {paymentId}", payment.Id);
    //     }
    // }
    //
    // public void UpdateCache(PaymentEntity payment)
    // {
    //     try
    //     {
    //         var cacheKey = $"{PaymentByIdCacheKeyPrefix}{payment.Id}";
    //         var jsonPayment = JsonSerializer.Serialize(payment, options);
    //         cache.SetString(cacheKey, jsonPayment, DefaultCacheDuration);
    //         
    //         if (payment.ExternalPaymentId != Guid.Empty)
    //         {
    //             var externalIdCacheKey = $"{ExternalPaymentCacheKeyPrefix}{payment.ExternalPaymentId}";
    //             cache.SetString(externalIdCacheKey, jsonPayment, DefaultCacheDuration);
    //         }
    //         
    //         cache.Remove(AllPaymentsCacheKey);
    //         cache.Remove(UnprocessedPaymentsCacheKey);
    //         cache.Remove(ExpiredPaymentsCacheKey);
    //         
    //         if (payment.Requisite?.UserId != null)
    //         {
    //             cache.Remove($"{UserPaymentsCacheKeyPrefix}{payment.Requisite.UserId}");
    //         }
    //         
    //         logger.LogDebug("Кеш для платежа {paymentId} был обновлен", payment.Id);
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, "Ошибка при обновлении кеша для платежа {paymentId}", payment.Id);
    //     }
    // }
    //
    // protected override void OnEntityUpdated(PaymentEntity entity)
    // {
    //     try
    //     {
    //         UpdateCache(entity);
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, "Ошибка при обработке обновления платежа {paymentId} в кеше", entity.Id);
    //     }
    // }
    //
    // protected override void OnEntityDeleted(PaymentEntity entity)
    // {
    //     try
    //     {
    //         InvalidateCache(entity);
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, "Ошибка при обработке удаления платежа {paymentId} из кеша", entity.Id);
    //     }
    // }
}