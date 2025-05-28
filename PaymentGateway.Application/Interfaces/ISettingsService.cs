namespace PaymentGateway.Application.Interfaces;

public interface ISettingsService
{
    Task<string?> GetValue(string key);
    Task<T?> GetValue<T>(string key);
    Task SetValue<T>(string key, T value);
}