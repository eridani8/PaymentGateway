using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Configs;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Infrastructure.Interfaces;

namespace PaymentGateway.Application.Services;

public class SettingsService(IUnitOfWork unit, JsonSerializerOptions jsonSerializerOptions)
    : ISettingsService
{
    public async Task<string?> GetValue(string key)
    {
        return await unit.SettingsRepository.GetValue(key);
    }

    public async Task<T?> GetValue<T>(string key)
    {
        var str = await GetValue(key);
        return str != null ? JsonSerializer.Deserialize<T>(str, jsonSerializerOptions) : default;
    }

    public async Task SetValue<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value, jsonSerializerOptions);

        var entity = await unit.SettingsRepository.GetFirstOrDefault(key);
        if (entity != null)
        {
            entity.Value = json;
            unit.SettingsRepository.Update(entity);
        }
        else
        {
            await unit.SettingsRepository.Add(new ConfigEntity()
            {
                Key = key,
                Value = json
            });
        }

        await unit.Commit();
    }
}