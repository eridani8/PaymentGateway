using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Interfaces;

namespace PaymentGateway.Infrastructure.Repositories.Cached;

public class CachedTransactionRepository(TransactionRepository repository, IMemoryCache cache) : ITransactionRepository
{
    private const string Key = "Transactions";
    private static string UserKey(Guid userId) => $"{Key}:User:{userId}";
    
    public DbSet<TransactionEntity> GetSet()
    {
        return repository.GetSet();
    }

    public Task Add(TransactionEntity entity)
    {
        return repository.Add(entity);
    }

    public void Update(TransactionEntity entity)
    {
        repository.Update(entity);
    }

    public void Delete(TransactionEntity entity)
    {
        repository.Delete(entity);
    }

    public async Task<List<TransactionEntity>> GetAllTransactions()
    {
        var result = await cache.GetOrCreateAsync(Key, entry =>
        {
            entry.SetSlidingExpiration(TimeSpan.FromSeconds(45));
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
            return repository.GetAllTransactions();
        });
    
        return result ?? [];
    }

    public async Task<List<TransactionEntity>> GetUserTransactions(Guid userId)
    {
        var result = await cache.GetOrCreateAsync(UserKey(userId), entry =>
        {
            entry.SetSlidingExpiration(TimeSpan.FromSeconds(45));
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
            return repository.GetUserTransactions(userId);
        });
    
        return result ?? [];
    }
}