using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentGateway.Core;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Shared.Enums;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace PaymentGateway.Infrastructure.Repositories;

public class RequisiteRepository(
    AppDbContext context, 
    IOptions<GatewaySettings> gatewaySettings, 
    ICache cache,
    ILogger<RequisiteRepository> logger,
    JsonSerializerOptions options)
    : RepositoryBase<RequisiteEntity>(context), IRequisiteRepository
{
    // private const string AllRequisitesCacheKey = "Requisites:All";
    // private const string UserRequisitesCacheKeyPrefix = "Requisites:User:";
    // private const string RequisiteByIdCacheKeyPrefix = "Requisite:";
    // private const string FreeRequisitesCacheKey = "Requisites:Free";
    // private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(5);

    public async Task<List<RequisiteEntity>> GetAll()
    {
        // var cachedRequisites = cache.GetString(AllRequisitesCacheKey);
        // if (!string.IsNullOrEmpty(cachedRequisites))
        // {
        //     try
        //     {
        //         return JsonSerializer.Deserialize<List<RequisiteEntity>>(cachedRequisites, options) ?? [];
        //     }
        //     catch (Exception ex)
        //     {
        //         logger.LogWarning(ex, "Ошибка при десериализации кешированных реквизитов");
        //         cache.Remove(AllRequisitesCacheKey);
        //     }
        // }
        //
        // var requisites = await Queryable().ToListAsync();
        //
        // try
        // {
        //     var jsonRequisites = JsonSerializer.Serialize(requisites, options);
        //     cache.SetString(AllRequisitesCacheKey, jsonRequisites, DefaultCacheDuration);
        // }
        // catch (Exception ex)
        // {
        //     logger.LogError(ex, "Ошибка при кешировании реквизитов");
        // }
        //
        // return requisites;
        
        return await Queryable().ToListAsync();
    }

    public async Task<List<RequisiteEntity>> GetFreeRequisites()
    {
        // var cachedRequisites = cache.GetString(FreeRequisitesCacheKey);
        // if (!string.IsNullOrEmpty(cachedRequisites))
        // {
        //     try
        //     {
        //         return JsonSerializer.Deserialize<List<RequisiteEntity>>(cachedRequisites, options) ?? [];
        //     }
        //     catch (Exception ex)
        //     {
        //         logger.LogWarning(ex, "Ошибка при десериализации кешированных свободных реквизитов");
        //         cache.Remove(FreeRequisitesCacheKey);
        //     }
        // }

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

        // try
        // {
        //     var jsonRequisites = JsonSerializer.Serialize(requisites, options);
        //     cache.SetString(FreeRequisitesCacheKey, jsonRequisites, TimeSpan.FromSeconds(30));
        // }
        // catch (Exception ex)
        // {
        //     logger.LogError(ex, "Ошибка при кешировании свободных реквизитов");
        // }

        return requisites;
    }

    public async Task<int> GetUserRequisitesCount(Guid userId)
    {
        // var cacheKey = $"{UserRequisitesCacheKeyPrefix}{userId}:Count";
        // var cachedCount = cache.GetString(cacheKey);
        //
        // if (!string.IsNullOrEmpty(cachedCount) && int.TryParse(cachedCount, out var count))
        // {
        //     return count;
        // }
        //
        // var result = await Queryable().CountAsync(r => r.UserId == userId);
        //
        // try
        // {
        //     cache.SetString(cacheKey, result.ToString(), DefaultCacheDuration);
        // }
        // catch (Exception ex)
        // {
        //     logger.LogError(ex, "Ошибка при кешировании количества реквизитов пользователя");
        // }
        //
        // return result;

        return await Queryable().CountAsync(r => r.UserId == userId);
    }

    public async Task<List<RequisiteEntity>> GetAllRequisites()
    {
        // var cachedRequisites = cache.GetString(AllRequisitesCacheKey);
        // if (!string.IsNullOrEmpty(cachedRequisites))
        // {
        //     try
        //     {
        //         return JsonSerializer.Deserialize<List<RequisiteEntity>>(cachedRequisites, options) ?? [];
        //     }
        //     catch (Exception ex)
        //     {
        //         logger.LogWarning(ex, "Ошибка при десериализации кешированных реквизитов");
        //         cache.Remove(AllRequisitesCacheKey);
        //     }
        // }
        //
        var requisites = await Queryable()
                .Include(r => r.Payment)
                .Include(r => r.User)
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        return requisites;
        //
        // try
        // {
        //     var jsonRequisites = JsonSerializer.Serialize(requisites, options);
        //     cache.SetString(AllRequisitesCacheKey, jsonRequisites, DefaultCacheDuration);
        // }
        // catch (Exception ex)
        // {
        //     logger.LogError(ex, "Ошибка при кешировании всех реквизитов");
        // }
        //
        // return requisites;
    }

    public async Task<List<RequisiteEntity>> GetUserRequisites(Guid userId)
    {
        // var cacheKey = $"{UserRequisitesCacheKeyPrefix}{userId}";
        // var cachedRequisites = cache.GetString(cacheKey);
        //
        // if (!string.IsNullOrEmpty(cachedRequisites))
        // {
        //     try
        //     {
        //         return JsonSerializer.Deserialize<List<RequisiteEntity>>(cachedRequisites, options) ?? [];
        //     }
        //     catch (Exception ex)
        //     {
        //         logger.LogWarning(ex, "Ошибка при десериализации кешированных реквизитов пользователя");
        //         cache.Remove(cacheKey);
        //     }
        // }

        var requisites = await Queryable()
                .Include(r => r.Payment)
                .Include(r => r.User)
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        return requisites;
        // try
        // {
        //     var jsonRequisites = JsonSerializer.Serialize(requisites, options);
        //     cache.SetString(cacheKey, jsonRequisites, DefaultCacheDuration);
        // }
        // catch (Exception ex)
        // {
        //     logger.LogError(ex, "Ошибка при кешировании реквизитов пользователя");
        // }
        //
        // return requisites;
    }

    public async Task<RequisiteEntity?> GetRequisiteById(Guid id)
    {
        // var cacheKey = $"{RequisiteByIdCacheKeyPrefix}{id}";
        // var cachedRequisite = cache.GetString(cacheKey);
        //
        // if (!string.IsNullOrEmpty(cachedRequisite))
        // {
        //     try
        //     {
        //         return JsonSerializer.Deserialize<RequisiteEntity>(cachedRequisite, options);
        //     }
        //     catch (Exception ex)
        //     {
        //         logger.LogWarning(ex, "Ошибка при десериализации кешированного реквизита");
        //         cache.Remove(cacheKey);
        //     }
        // }

        var requisite = await Queryable()
                .Include(r => r.Payment)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        return requisite;
        // if (requisite != null)
        // {
        //     try
        //     {
        //         var jsonRequisite = JsonSerializer.Serialize(requisite, options);
        //         cache.SetString(cacheKey, jsonRequisite, DefaultCacheDuration);
        //     }
        //     catch (Exception ex)
        //     {
        //         logger.LogError(ex, "Ошибка при кешировании реквизита по ID");
        //     }
        // }
        //
        // return requisite;
    }

    public async Task<RequisiteEntity?> HasSimilarRequisite(string paymentData)
    {
        return await Queryable()
                .FirstOrDefaultAsync(r => 
                    r.PaymentData.Equals(paymentData));
    }

    // public void InvalidateCache(RequisiteEntity requisite)
    // {
    //     try
    //     {
    //         cache.Remove($"{RequisiteByIdCacheKeyPrefix}{requisite.Id}");
    //         
    //         cache.Remove($"{UserRequisitesCacheKeyPrefix}{requisite.UserId}");
    //         cache.Remove($"{UserRequisitesCacheKeyPrefix}{requisite.UserId}:Count");
    //         
    //         cache.Remove(AllRequisitesCacheKey);
    //         cache.Remove(FreeRequisitesCacheKey);
    //         
    //         logger.LogDebug("Кеш для реквизита {requisiteId} был инвалидирован", requisite.Id);
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, "Ошибка при инвалидации кеша для реквизита {requisiteId}", requisite.Id);
    //     }
    // }
    //
    // public void UpdateCache(RequisiteEntity requisite)
    // {
    //     try
    //     {
    //         var cacheKey = $"{RequisiteByIdCacheKeyPrefix}{requisite.Id}";
    //         var jsonRequisite = JsonSerializer.Serialize(requisite, options);
    //         cache.SetString(cacheKey, jsonRequisite, DefaultCacheDuration);
    //         
    //         cache.Remove($"{UserRequisitesCacheKeyPrefix}{requisite.UserId}");
    //         cache.Remove(FreeRequisitesCacheKey);
    //         cache.Remove(AllRequisitesCacheKey);
    //         
    //         logger.LogDebug("Кеш для реквизита {requisiteId} был обновлен", requisite.Id);
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, "Ошибка при обновлении кеша для реквизита {requisiteId}", requisite.Id);
    //     }
    // }
    
    // protected override void OnEntityUpdated(RequisiteEntity entity)
    // {
    //     try
    //     {
    //         UpdateCache(entity);
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, "Ошибка при обработке обновления реквизита {requisiteId} в кеше", entity.Id);
    //     }
    // }
    //
    // protected override void OnEntityDeleted(RequisiteEntity entity)
    // {
    //     try
    //     {
    //         InvalidateCache(entity);
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, "Ошибка при обработке удаления реквизита {requisiteId} из кеша", entity.Id);
    //     }
    // }
}