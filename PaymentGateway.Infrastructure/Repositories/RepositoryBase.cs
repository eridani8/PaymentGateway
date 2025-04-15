using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;
using System.Linq.Expressions;

namespace PaymentGateway.Infrastructure.Repositories;

public class RepositoryBase<TEntity>(AppDbContext context, ICache cache)
    : IRepositoryBase<TEntity> where TEntity : class, ICacheable
{
    private readonly DbSet<TEntity> _entities = context.Set<TEntity>();
    
    public IQueryable<TEntity> QueryableGetAll()
    {
        return _entities;
    }
    
    public async Task<List<TEntity>> GetAll()
    {
        // var prefix = InMemoryCache.GetCacheKey<TEntity>();
        //
        // var cachedKeys = cache.AllKeys()
        //     .Where(k => k.StartsWith(prefix))
        //     .ToList();
        //
        // var cachedItems = cachedKeys
        //     .Select(cache.Get<TEntity>)
        //     .Where(entity => entity != null)
        //     .Cast<TEntity>()
        //     .ToList();
        //
        // if (cachedKeys.Count > 0 && cachedItems.Count == cachedKeys.Count)
        // {
        //     return cachedItems;
        // }
        
        var entities = await _entities.ToListAsync();
        // foreach (var entity in entities)
        // {
        //     cache.Set(entity, InMemoryCache.DefaultExpiration);
        // } // TODO cache

        return entities;
    }

    public async Task<TEntity?> GetById(Guid id, params Expression<Func<TEntity, object?>>[] includes)
    {
        // var key = InMemoryCache.GetCacheKey<TEntity>(id);
        
        // var cached = cache.Get<TEntity>(key);
        // if (cached is not null)
        // {
        //     return cached;
        // }

        if (includes.Length == 0)
        {
            var entity = await _entities.FindAsync(id);
            // if (entity is not null)
            // {
            //     cache.Set(key, entity, InMemoryCache.DefaultExpiration);
            // } // TODO cache
            
            return entity;
        }
        
        var query = _entities.AsQueryable();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        
        var entityWithIncludes = await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
        
        // if (entityWithIncludes is not null)
        // {
        //     cache.Set(key, entityWithIncludes, InMemoryCache.DefaultExpiration);
        // } // TODO cache
        
        return entityWithIncludes;
    }

    public async Task Add(TEntity entity)
    {
        await _entities.AddAsync(entity); 
        
        // cache.Set(entity, InMemoryCache.DefaultExpiration); // TODO cache
    }

    public void Update(TEntity entity)
    {
        _entities.Update(entity);
        
        // cache.Set(entity, InMemoryCache.DefaultExpiration); // TODO cache
    }

    public void Delete(TEntity entity)
    {
        _entities.Remove(entity);
        
        // cache.Remove(entity); // TODO cache
    }
}