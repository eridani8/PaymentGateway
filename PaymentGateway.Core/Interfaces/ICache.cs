namespace PaymentGateway.Core.Interfaces;

public interface ICache
{
    void Set<T>(string key, T obj, TimeSpan? expiry = null);
    void Set<T>(T obj, TimeSpan? expiry = null);
    T? Get<T>(string key);
    void Remove(string key);
    void Remove<T>(T obj);
}