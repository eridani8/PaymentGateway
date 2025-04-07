namespace PaymentGateway.Core.Interfaces;

public interface IRepositoryBase<TEntity> where TEntity : class
{
    IQueryable<TEntity> QueryableGetAll();
    Task<IEnumerable<TEntity>> GetAll();
    Task<TEntity?> GetById(Guid id);
    Task Add(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
}