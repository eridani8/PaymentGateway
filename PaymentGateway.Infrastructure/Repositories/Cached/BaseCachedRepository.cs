using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Interfaces;

namespace PaymentGateway.Infrastructure.Repositories.Cached;

public abstract class BaseCachedRepository<TEntity, TRepository>(TRepository repository, IMemoryCache cache) 
    where TEntity : BaseEntity
    where TRepository : IRepositoryBase<TEntity>
{
    protected readonly TRepository Repository = repository;
    protected abstract string CacheKeyPrefix { get; }
    
    protected string GetFullCacheKey(string suffix = "") => 
        string.IsNullOrEmpty(suffix) ? CacheKeyPrefix : $"{CacheKeyPrefix}:{suffix}";
    
    protected string GetUserKey(Guid userId) => GetFullCacheKey($"User:{userId}");
    
    public DbSet<TEntity> GetSet()
    {
        return Repository.GetSet();
    }

    public virtual Task Add(TEntity entity)
    {
        InvalidateCache();
        return Repository.Add(entity);
    }

    public virtual void Update(TEntity entity)
    {
        InvalidateCache();
        Repository.Update(entity);
    }

    public virtual void Delete(TEntity entity)
    {
        InvalidateCache();
        Repository.Delete(entity);
    }
    
    protected void InvalidateCache(Guid? userId = null)
    {
        cache.Remove(GetFullCacheKey());
        if (userId.HasValue)
        {
            cache.Remove(GetUserKey(userId.Value));
        }
    }
    
    protected async Task<List<TResult>> GetCachedData<TResult>(
        string cacheKey, 
        Func<Task<List<TResult>>> dataLoader,
        TimeSpan? slidingExpiration = null,
        TimeSpan? absoluteExpiration = null)
    {
        var result = await cache.GetOrCreateAsync(cacheKey, entry =>
        {
            entry.SetSlidingExpiration(slidingExpiration ?? TimeSpan.FromMinutes(1));
            entry.SetAbsoluteExpiration(absoluteExpiration ?? TimeSpan.FromMinutes(5));
            return dataLoader();
        });
    
        return result ?? [];
    }
} 