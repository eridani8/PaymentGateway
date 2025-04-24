using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Interfaces;

namespace PaymentGateway.Infrastructure.Repositories.Cached;

public class CachedPaymentRepository(PaymentRepository repository, IMemoryCache cache) : IPaymentRepository
{
    private const string Key = "Payments";
    private static string UserKey(Guid userId) => $"{Key}:User:{userId}";
    
    public DbSet<PaymentEntity> GetSet()
    {
        return repository.GetSet();
    }

    public Task Add(PaymentEntity entity)
    {
        cache.Remove(Key);
        cache.Remove(UserKey(entity.Id));
        return repository.Add(entity);
    }

    public void Update(PaymentEntity entity)
    {
        cache.Remove(Key);
        cache.Remove(UserKey(entity.Id));
        repository.Update(entity);
    }

    public void Delete(PaymentEntity entity)
    {
        cache.Remove(Key);
        cache.Remove(UserKey(entity.Id));
        repository.Delete(entity);
    }

    public Task<List<PaymentEntity>> GetUnprocessedPayments()
    {
        return repository.GetUnprocessedPayments();
    }

    public Task<List<PaymentEntity>> GetExpiredPayments()
    {
        return repository.GetExpiredPayments();
    }

    public Task<PaymentEntity?> GetExistingPayment(Guid externalPaymentId)
    {
        return repository.GetExistingPayment(externalPaymentId);
    }

    public Task<PaymentEntity?> PaymentById(Guid id)
    {
        return repository.PaymentById(id);
    }

    public async Task<List<PaymentEntity>> GetAllPayments()
    {
        var result = await cache.GetOrCreateAsync(Key, entry =>
        {
            entry.SetSlidingExpiration(TimeSpan.FromSeconds(45));
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
            return repository.GetAllPayments();
        });
    
        return result ?? [];
    }

    public async Task<List<PaymentEntity>> GetUserPayments(Guid userId)
    {
        var result = await cache.GetOrCreateAsync(UserKey(userId), entry =>
        {
            entry.SetSlidingExpiration(TimeSpan.FromSeconds(45));
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
            return repository.GetUserPayments(userId);
        });
    
        return result ?? [];
    }
}