namespace PaymentGateway.Core.Interfaces;

public interface IRepositoryBase<TEntity> where TEntity : class
{
    IQueryable<TEntity> Queryable();
    Task Add(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
}