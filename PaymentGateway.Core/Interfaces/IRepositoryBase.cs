namespace PaymentGateway.Core.Interfaces;
using System.Linq.Expressions;

public interface IRepositoryBase<TEntity> where TEntity : class
{
    IQueryable<TEntity> QueryableGetAll();
    Task<List<TEntity>> GetAll();
    Task<TEntity?> GetById(Guid id, params Expression<Func<TEntity, object?>>[] includes);
    Task Add(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
}