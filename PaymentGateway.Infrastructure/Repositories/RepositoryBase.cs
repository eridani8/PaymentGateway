using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;

namespace PaymentGateway.Infrastructure.Repositories;

public class RepositoryBase<TEntity>(AppDbContext context) : IRepositoryBase<TEntity> where TEntity : class
{
    protected readonly AppDbContext Context = context;
    private readonly DbSet<TEntity> _entities = context.Set<TEntity>();

    public IQueryable<TEntity> GetAll()
    {
        return _entities;
    }

    public async Task<TEntity?> GetById(Guid id)
    {
        return await _entities.FindAsync(id);
    }

    public async Task Add(TEntity entity)
    {
        await _entities.AddAsync(entity);
    }

    public void Update(TEntity entity)
    {
        _entities.Update(entity);
    }

    public void Delete(TEntity entity)
    {
        _entities.Remove(entity);
    }
}