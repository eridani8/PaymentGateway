using StackExchange.Redis;

namespace PaymentGateway.Core.Interfaces;

public interface ICache
{
    IConnectionMultiplexer Multiplexer { get; }
    Task Set<T>(string key, T obj, TimeSpan? expiry = null);
    Task Set<T>(T obj, TimeSpan? expiry = null);
    Task<T?> Get<T>(string key);
    Task Remove(string key);
}