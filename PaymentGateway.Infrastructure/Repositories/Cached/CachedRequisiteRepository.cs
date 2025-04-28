using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Interfaces;

namespace PaymentGateway.Infrastructure.Repositories.Cached;

public class CachedRequisiteRepository(RequisiteRepository repository, IMemoryCache cache) 
    : BaseCachedRepository<RequisiteEntity, RequisiteRepository>(repository, cache), IRequisiteRepository
{
    protected override string CacheKeyPrefix => "Requisites";

    public override Task Add(RequisiteEntity entity)
    {
        InvalidateCache(entity.UserId);
        return Repository.Add(entity);
    }

    public override void Update(RequisiteEntity entity)
    {
        InvalidateCache(entity.UserId);
        Repository.Update(entity);
    }

    public override void Delete(RequisiteEntity entity)
    {
        InvalidateCache(entity.UserId);
        Repository.Delete(entity);
    }

    public Task<List<RequisiteEntity>> GetAllTracked()
    {
        return Repository.GetAllTracked();
    }

    public Task<List<RequisiteEntity>> GetFreeRequisites(int count)
    {
        return Repository.GetFreeRequisites(count);
    }

    public Task<int> GetUserRequisitesCount(Guid userId)
    {
        return Repository.GetUserRequisitesCount(userId);
    }

    public Task<List<RequisiteEntity>> GetAllRequisites()
    {
        return GetCachedData(GetFullCacheKey(), Repository.GetAllRequisites);
    }

    public Task<List<RequisiteEntity>> GetUserRequisites(Guid userId)
    {
        return GetCachedData(GetUserKey(userId), () => Repository.GetUserRequisites(userId));
    }

    public Task<RequisiteEntity?> GetRequisiteById(Guid id)
    {
        return Repository.GetRequisiteById(id);
    }

    public Task<RequisiteEntity?> HasSimilarRequisite(string paymentData)
    {
        return Repository.HasSimilarRequisite(paymentData);
    }
}