using PaymentGateway.Core.Entities;

namespace PaymentGateway.Infrastructure.Interfaces;

public interface ISettingsRepository
{
    Task<string?> GetValue(string key);
    Task<ConfigEntity?> GetFirstOrDefault(string key);
    void Update(ConfigEntity entity);
    Task Add(ConfigEntity entity);
}