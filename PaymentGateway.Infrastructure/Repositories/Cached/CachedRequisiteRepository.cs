using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Interfaces;

namespace PaymentGateway.Infrastructure.Repositories.Cached;

public class CachedRequisiteRepository(RequisiteRepository repository, IMemoryCache cache) : IRequisiteRepository
{
    private const string Key = "Requisites";
    private static string UserKey(Guid userId) => $"{Key}:User:{userId}";
    
    public DbSet<RequisiteEntity> GetSet()
    {
        return repository.GetSet();
    }

    public Task Add(RequisiteEntity entity)
    {
        cache.Remove(Key);
        cache.Remove(UserKey(entity.UserId));
        return repository.Add(entity);
    }

    public void Update(RequisiteEntity entity)
    {
        cache.Remove(Key);
        cache.Remove(UserKey(entity.UserId));
        repository.Update(entity);
    }

    public void Delete(RequisiteEntity entity)
    {
        cache.Remove(Key);
        cache.Remove(UserKey(entity.UserId));
        repository.Delete(entity);
    }

    public Task<List<RequisiteEntity>> GetAllTracked()
    {
        return repository.GetAllTracked();
    }

    public Task<List<RequisiteEntity>> GetFreeRequisites()
    {
        return repository.GetFreeRequisites();
    }

    public Task<int> GetUserRequisitesCount(Guid userId)
    {
        return repository.GetUserRequisitesCount(userId);
    }

    public async Task<List<RequisiteEntity>> GetAllRequisites()
    {
        var result = await cache.GetOrCreateAsync(Key, entry =>
        {
            entry.SetSlidingExpiration(TimeSpan.FromSeconds(45));
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
            return repository.GetFreeRequisites();
        });
    
        return result ?? [];
    }

    public async Task<List<RequisiteEntity>> GetUserRequisites(Guid userId)
    {
        var result = await cache.GetOrCreateAsync(UserKey(userId), entry =>
        {
            entry.SetSlidingExpiration(TimeSpan.FromSeconds(45));
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
            return repository.GetUserRequisites(userId);
        });
    
        return result ?? [];
    }

    public Task<RequisiteEntity?> GetRequisiteById(Guid id)
    {
        return repository.GetRequisiteById(id);
    }

    public Task<RequisiteEntity?> HasSimilarRequisite(string paymentData)
    {
        return repository.HasSimilarRequisite(paymentData);
    }
}