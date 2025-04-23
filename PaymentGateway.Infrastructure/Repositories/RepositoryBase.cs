using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;
using System.Linq.Expressions;

namespace PaymentGateway.Infrastructure.Repositories;

public class RepositoryBase<TEntity>(AppDbContext context)
    : IRepositoryBase<TEntity> where TEntity : class, ICacheable
{
    private readonly DbSet<TEntity> _entities = context.Set<TEntity>();
    
    public IQueryable<TEntity> Queryable()
    {
        return _entities;
    }

    public async Task Add(TEntity entity)
    {
        await _entities.AddAsync(entity); 
    }

    public void Update(TEntity entity)
    {
        _entities.Update(entity);
        // OnEntityUpdated(entity);
    }

    public void Delete(TEntity entity)
    {
        _entities.Remove(entity);
        // OnEntityDeleted(entity);
    }
    
    // protected virtual void OnEntityUpdated(TEntity entity)
    // {
    // }
    //
    // protected virtual void OnEntityDeleted(TEntity entity)
    // {
    // }
}