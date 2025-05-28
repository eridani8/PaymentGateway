using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Infrastructure.Interfaces;

namespace PaymentGateway.Infrastructure.Repositories;

public class SettingsRepository(AppDbContext context)
    : RepositoryBase<ConfigEntity>(context), ISettingsRepository
{
    public async Task<string?> GetValue(string key)
    {
        var entity = await GetSet()
            .FirstOrDefaultAsync(k => k.Key == key);
        return entity?.Value;
    }

    public async Task<ConfigEntity?> GetFirstOrDefault(string key)
    {
        return await GetSet()
            .FirstOrDefaultAsync(k => k.Key == key);
    }
}