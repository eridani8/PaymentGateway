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
    
    protected string GetCacheKey(string suffix = "") => 
        string.IsNullOrEmpty(suffix) ? CacheKeyPrefix : $"{CacheKeyPrefix}:{suffix}";
    
    protected string GetUserKey(Guid userId) => GetCacheKey($"User:{userId}");

    protected string SystemKey => GetCacheKey("System");

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

    public void InvalidateCache(Guid? id = null)
    {
        InvalidateKeyCache();
        if (id.HasValue)
        {
            InvalidateUserCache(id.Value);
        }
    }
    
    private void InvalidateKeyCache()
    {
        cache.Remove(GetCacheKey());
    }
    
    private void InvalidateUserCache(Guid userId)
    {
        cache.Remove(GetUserKey(userId));
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
    
    protected async Task<TResult?> GetCachedData<TResult>(
        string cacheKey, 
        Func<Task<TResult>> dataLoader,
        TimeSpan? slidingExpiration = null,
        TimeSpan? absoluteExpiration = null)
    {
        var result = await cache.GetOrCreateAsync(cacheKey, entry =>
        {
            entry.SetSlidingExpiration(slidingExpiration ?? TimeSpan.FromMinutes(1));
            entry.SetAbsoluteExpiration(absoluteExpiration ?? TimeSpan.FromMinutes(5));
            return dataLoader();
        });
    
        return result;
    }
} 