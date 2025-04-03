using System.Linq.Expressions;

namespace PaymentGateway.Core.Interfaces;

public interface IRepositoryBase<TEntity> where TEntity : class
{
    IQueryable<TEntity> GetAll();
    Task<TEntity?> GetById(Guid id);
    Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> predicate);
    Task Add(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
}