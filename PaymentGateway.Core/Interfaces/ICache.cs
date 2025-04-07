using System.Collections.Concurrent;
using System.Text.Json;

namespace PaymentGateway.Core.Interfaces;

public interface ICache
{
    JsonSerializerOptions Options { get; }
    ConcurrentDictionary<string, byte> Keys { get; }
    IEnumerable<string> AllKeys();
    void Set<T>(string key, T obj, TimeSpan? expiry = null) where T : ICacheable;
    void Set<T>(T obj, TimeSpan? expiry = null) where T : ICacheable;
    T? Get<T>(string key);
    void Remove(string key);
    void Remove<T>(T obj) where T : ICacheable;
    bool Exists(string key);
    IEnumerable<T> GetByPrefix<T>(string prefix);
}