using System.Text.Json;
using PaymentGateway.Core.Interfaces;
using StackExchange.Redis;

namespace PaymentGateway.Infrastructure.Data;

public class RedisCache(IConnectionMultiplexer multiplexer) : ICache
{
    public IConnectionMultiplexer Multiplexer { get; } = multiplexer;
    
    public async Task Set<T>(string key, T obj, TimeSpan? expiry = null)
    {
        var db = Multiplexer.GetDatabase();
        var json = JsonSerializer.Serialize(obj);
        await db.StringSetAsync(key, json, expiry);
    }

    public async Task Set<T>(T obj, TimeSpan? expiry = null)
    {
        if (!CacheHelper<T>.TryGetEntityId(obj, out var id)) return;
        var key = CacheHelper<T>.GetCacheKey(id);
        await Set(key, obj, expiry);
    }
    
    public async Task<T?> Get<T>(string key)
    {
        var db = Multiplexer.GetDatabase();
        var value = await db.StringGetAsync(key);
        if (value.IsNull)
        {
            return default;
        }
        return JsonSerializer.Deserialize<T>(value.ToString());
    }

    public async Task Remove(string key)
    {
        var db = Multiplexer.GetDatabase();
        await db.KeyDeleteAsync(key);
    }
}