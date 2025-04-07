using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;

namespace PaymentGateway.Infrastructure.Repositories;

public class RepositoryBase<TEntity>(AppDbContext context, ICache cache)
    : IRepositoryBase<TEntity> where TEntity : class, ICacheable
{
    private readonly DbSet<TEntity> _entities = context.Set<TEntity>();

    public IQueryable<TEntity> QueryableGetAll()
    {
        return _entities;
    }

    public async Task<IEnumerable<TEntity>> GetAll()
    {
        var prefix = InMemoryCache.GetCacheKey<TEntity>();
        
        var cachedItems = cache.GetByPrefix<TEntity>(prefix).ToList();
        if (cachedItems.Count > 0)
        {
            return cachedItems;
        }
        
        var entities = await _entities.ToListAsync();
        foreach (var entity in entities)
        {
            cache.Set(entity);
        }

        return entities;
    }

    public async Task<TEntity?> GetById(Guid id)
    {
        var key = InMemoryCache.GetCacheKey<TEntity>(id);
        var cached = cache.Get<TEntity>(key);
        if (cached is not null)
        {
            return cached;
        }

        var entity = await _entities.FindAsync(id);
        if (entity is not null)
        {
            cache.Set(key, entity);
        }

        return entity;
    }

    public async Task Add(TEntity entity)
    {
        await _entities.AddAsync(entity); 
        
        cache.Set(entity);
    }

    public void Update(TEntity entity)
    {
        _entities.Update(entity);
        
        cache.Set(entity);
    }

    public void Delete(TEntity entity)
    {
        _entities.Remove(entity);
        
        cache.Remove(entity);
    }
}