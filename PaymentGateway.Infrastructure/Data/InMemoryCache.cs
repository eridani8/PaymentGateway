using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Infrastructure.Data;

public class InMemoryCache(IMemoryCache cache) : ICache
{
    public JsonSerializerOptions Options { get; } = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve
    };

    public void Set<T>(string key, T obj, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(obj, Options);
        var cacheOptions = expiry.HasValue
            ? new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry }
            : null;

        cache.Set(key, json, cacheOptions);
    }

    public void Set<T>(T obj, TimeSpan? expiry = null)
    {
        if (CacheHelper<T>.TryGetEntityId(obj, out var id))
        {
            Set(CacheHelper<T>.GetCacheKey(id), obj, expiry);
        }
    }
    
    public T? Get<T>(string key)
    {
        var json = cache.Get<string>(key);
        return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json, Options);
    }

    public void Remove(string key)
    {
        cache.Remove(key);
    }

    public void Remove<T>(T obj)
    {
        if (CacheHelper<T>.TryGetEntityId(obj, out var id))
        {
            Remove(CacheHelper<T>.GetCacheKey(id));
        }
    }
}