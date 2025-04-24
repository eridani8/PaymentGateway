using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Infrastructure.Interfaces;

public interface IRepositoryBase<TEntity> where TEntity : BaseEntity
{
    DbSet<TEntity> GetSet();
    Task Add(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
}