using Microsoft.EntityFrameworkCore;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Interfaces;

namespace PaymentGateway.Infrastructure.Repositories;

public class RepositoryBase<TEntity>(AppDbContext context)
    : IRepositoryBase<TEntity> where TEntity : BaseEntity
{
    public DbSet<TEntity> GetSet()
    {
        return context.Set<TEntity>();
    }
    
    public async Task Add(TEntity entity)
    {
        await GetSet().AddAsync(entity); 
    }

    public void Update(TEntity entity)
    {
        GetSet().Update(entity);
    }

    public void Delete(TEntity entity)
    {
        GetSet().Remove(entity);
    }
}