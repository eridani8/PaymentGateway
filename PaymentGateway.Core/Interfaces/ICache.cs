using System.Collections.Concurrent;
using System.Text.Json;

namespace PaymentGateway.Core.Interfaces;

public interface ICache
{
    ConcurrentDictionary<string, CacheMetadata> Keys { get; }
    IEnumerable<string> AllKeys();
    void Set<T>(string key, T obj, TimeSpan? expiry = null) where T : ICacheable;
    void Set<T>(T obj, TimeSpan? expiry = null) where T : ICacheable;
    void Set(Type type, Guid id, object obj, TimeSpan? expiry = null);
    void Set(string key, TimeSpan? expiry = null);
    T? Get<T>(string key);
    void Remove(string key);
    void Remove<T>(T obj) where T : ICacheable;
    void Remove(Type type, Guid id);
    bool Exists(string key);
    IEnumerable<T> GetByPrefix<T>(string prefix);
    TimeSpan? GetRemainingLifetime(string key);
}