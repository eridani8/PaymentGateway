using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Interfaces;

namespace PaymentGateway.Infrastructure.Repositories.Cached;

public class CachedTransactionRepository(TransactionRepository repository, IMemoryCache cache) 
    : BaseCachedRepository<TransactionEntity, TransactionRepository>(repository, cache), ITransactionRepository
{
    protected override string CacheKeyPrefix => "Transactions";
    
    public override Task Add(TransactionEntity entity)
    {
        InvalidateCache();
        return Repository.Add(entity);
    }

    public override void Update(TransactionEntity entity)
    {
        InvalidateCache();
        Repository.Update(entity);
    }

    public override void Delete(TransactionEntity entity)
    {
        InvalidateCache();
        Repository.Delete(entity);

    }

    public Task<List<TransactionEntity>> GetAllTransactions()
    {
        return GetCachedData(GetFullCacheKey(), Repository.GetAllTransactions);
    }

    public Task<List<TransactionEntity>> GetUserTransactions(Guid userId)
    {
        return GetCachedData(GetUserKey(userId), () => Repository.GetUserTransactions(userId));
    }
}