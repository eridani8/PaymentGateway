namespace PaymentGateway.Core.Interfaces;

public interface IRepositoryBase<TEntity> where TEntity : class
{
    // Task<List<TEntity>> Find(Func<TEntity, bool> predicate);
    IQueryable<TEntity> QueryableGetAll();
    Task<List<TEntity>> GetAll();
    Task<TEntity?> GetById(Guid id);
    Task Add(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
}