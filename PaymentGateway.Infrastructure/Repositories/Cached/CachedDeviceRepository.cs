using Microsoft.Extensions.Caching.Memory;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Interfaces;

namespace PaymentGateway.Infrastructure.Repositories.Cached;

public class CachedDeviceRepository(DeviceRepository repository, IMemoryCache cache)
    : BaseCachedRepository<DeviceEntity, DeviceRepository>(repository, cache), IDeviceRepository
{
    protected override string CacheKeyPrefix => "Devices";

    public Task<DeviceEntity?> GetDeviceById(Guid id)
    {
        return Repository.GetDeviceById(id);
    }

    public Task<List<DeviceEntity>> GetAllDevices()
    {
        return GetCachedData(GetCacheKey(), Repository.GetAllDevices);
    }

    public Task<List<DeviceEntity>> GetUserDevices(Guid userId)
    {
        return GetCachedData(GetUserKey(userId), () => Repository.GetUserDevices(userId));
    }
}