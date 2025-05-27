using Microsoft.Extensions.Caching.Memory;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Interfaces;

namespace PaymentGateway.Infrastructure.Repositories.Cached;

public class CachedPaymentRepository(PaymentRepository repository, IMemoryCache cache) 
    : BaseCachedRepository<PaymentEntity, PaymentRepository>(repository, cache), IPaymentRepository
{
    protected override string CacheKeyPrefix => "Payments";
    
    public override Task Add(PaymentEntity entity)
    {
        InvalidateCache(entity.UserId);
        return Repository.Add(entity);
    }

    public override void Update(PaymentEntity entity)
    {
        InvalidateCache(entity.UserId);
        Repository.Update(entity);
    }

    public override void Delete(PaymentEntity entity)
    {
        InvalidateCache(entity.UserId);
        Repository.Delete(entity);
    }

    public Task<List<PaymentEntity>> GetUnprocessedPayments()
    {
        return Repository.GetUnprocessedPayments();
    }

    public Task<List<PaymentEntity>> GetExpiredPayments()
    {
        return Repository.GetExpiredPayments();
    }

    public Task<PaymentEntity?> PaymentById(Guid id)
    {
        return Repository.PaymentById(id);
    }

    public Task<List<PaymentEntity>> GetAllPayments()
    {
        return GetCachedData(GetCacheKey(), Repository.GetAllPayments);
    }

    public Task<List<PaymentEntity>> GetUserPayments(Guid userId)
    {
        return GetCachedData(GetUserKey(userId), () => Repository.GetUserPayments(userId));
    }
}