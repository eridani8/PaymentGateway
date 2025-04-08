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
    
    private void SetCacheInternal(string key, string json, TimeSpan? expiry)
    {
        var cacheOptions = expiry.HasValue
            ? new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry }
            : new MemoryCacheEntryOptions();

        cacheOptions.RegisterPostEvictionCallback((cacheKey, _, _, _) =>
        {
            if (cacheKey is string keyStr && !string.IsNullOrEmpty(keyStr))
            {
                Keys.TryRemove(keyStr, out _);
            }
        });
        
        cache.Set(key, json, cacheOptions);
        Keys.TryAdd(key, 0);
    }

    public void Set<T>(string key, T obj, TimeSpan? expiry = null) where T : ICacheable
    {
        var json = JsonSerializer.Serialize(obj, Options);
        SetCacheInternal(key, json, expiry);
    }

    public void Set<T>(T obj, TimeSpan? expiry = null) where T : ICacheable
    {
        Set(GetCacheKey<T>(obj.Id), obj, expiry);
    }

    public void Set(Type type, Guid id, object obj, TimeSpan? expiry = null)
    {
        var key = GetCacheKey(type, id);
        var json = JsonSerializer.Serialize(obj, Options);
        SetCacheInternal(key, json, expiry);
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

    public void Remove<T>(T obj) where T : ICacheable
    {
        Remove(GetCacheKey<T>(obj.Id));
    }
    
    public void Remove(Type type, Guid id)
    {
        var key = GetCacheKey(type, id);
        Remove(key);
    }

    public bool Exists(string key)
    {
        return Keys.ContainsKey(key);
    }

    public IEnumerable<T> GetByPrefix<T>(string prefix)
    {
        foreach (var key in Keys.Keys.Where(k => k.StartsWith(prefix)))
        {
            var entity = Get<T>(key);
            if (entity is not null)
            {
                yield return entity;
            }
        }
    }

    public static string GetCacheKey<T>(Guid id)
    {
        return $"{GetCacheKey<T>()}:{id}";
    }

    public static string GetCacheKey<T>()
    {
        return $"{typeof(T).Name}";
    }
    
    public static string GetCacheKey(Type entityType, Guid id)
    {
        return $"{entityType.Name}:{id}";
    }
}