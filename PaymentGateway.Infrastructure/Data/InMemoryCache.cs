using System.Collections.Concurrent;
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

    public ConcurrentDictionary<string, byte> Keys { get; } = new();

    public IEnumerable<string> AllKeys()
    {
        return Keys.Keys;
    }

    public void Set<T>(string key, T obj, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(obj, Options);
        var cacheOptions = expiry.HasValue
            ? new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry }
            : new MemoryCacheEntryOptions();

        cacheOptions.RegisterPostEvictionCallback((cacheKey, value, reason, state) =>
        {
            if (cacheKey is string keyStr && !string.IsNullOrEmpty(keyStr))
            {
                Keys.TryRemove(keyStr, out _);
            }
        });

        cache.Set(key, json, cacheOptions);
        Keys.TryAdd(key, 0);
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
        Keys.TryRemove(key, out _);
    }

    public void Remove<T>(T obj)
    {
        if (CacheHelper<T>.TryGetEntityId(obj, out var id))
        {
            Remove(CacheHelper<T>.GetCacheKey(id));
        }
    }
}