using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PaymentGateway.Core;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Infrastructure.Data;

public class InMemoryCache(IMemoryCache cache, ILogger<InMemoryCache> logger) : ICache
{
    public static TimeSpan DefaultExpiration = TimeSpan.FromSeconds(30);
    
    public JsonSerializerOptions Options { get; } = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve
    };

    public ConcurrentDictionary<string, CacheMetadata> Keys { get; } = new();

    public IEnumerable<string> AllKeys()
    {
        return Keys.Keys;
    }
    
    private void SetCacheInternal(string key, object obj, TimeSpan? expiry)
    {
        var effectiveExpiry = expiry;
        DateTime? absoluteExpiration = null;
        
        if (Keys.TryGetValue(key, out var metadata) && metadata.OriginalExpiry.HasValue && expiry == null)
        {
            effectiveExpiry = metadata.OriginalExpiry;
            
            if (metadata.ExpiryTime.HasValue && metadata.ExpiryTime.Value > DateTime.Now)
            {
                absoluteExpiration = metadata.ExpiryTime.Value;
            }
        }
        
        var cacheOptions = new MemoryCacheEntryOptions();
        
        if (absoluteExpiration.HasValue)
        {
            cacheOptions.AbsoluteExpiration = absoluteExpiration.Value;
        }
        else if (effectiveExpiry.HasValue)
        {
            cacheOptions.AbsoluteExpirationRelativeToNow = effectiveExpiry.Value;
        }
        
        DateTime? expiryTime = effectiveExpiry.HasValue 
            ? DateTime.Now.Add(effectiveExpiry.Value) 
            : null;
        
        var newMetadata = new CacheMetadata
        {
            OriginalExpiry = effectiveExpiry,
            ExpiryTime = expiryTime
        };
        
        cacheOptions.RegisterPostEvictionCallback((cacheKey, _, _, _) =>
        {
            var cacheKeyStr = cacheKey.ToString()!;
            Keys.TryRemove(cacheKeyStr, out _);
            logger.LogInformation("Cache removed: {key}", cacheKeyStr);
        });
        
        cache.Set(key, obj, cacheOptions);
        Keys[key] = newMetadata;
        logger.LogInformation("Cache {operation}: {key}", Keys.ContainsKey(key) ? "updated" : "added", key);
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

    public void Set(string key, TimeSpan? expiry = null)
    {
        SetCacheInternal(key, "", expiry);
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
    
    public TimeSpan? GetRemainingLifetime(string key)
    {
        if (!Keys.TryGetValue(key, out var metadata) || !metadata.ExpiryTime.HasValue) return null;
        var remaining = metadata.ExpiryTime.Value - DateTime.Now;
        return remaining.TotalMilliseconds > 0 ? remaining : TimeSpan.Zero;
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