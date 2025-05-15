using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Infrastructure.Interfaces;

namespace PaymentGateway.Infrastructure.Repositories;

public class DeviceRepository(
    AppDbContext context)
    : RepositoryBase<DeviceEntity>(context), IDeviceRepository
{
    public async Task<List<DeviceEntity>> GetAllDevices()
    {
        return await GetSet()
            .AsNoTracking()
            .OrderByDescending(d => d.BindingAt)
            .ToListAsync();
    }

    public async Task<List<DeviceEntity>> GetUserDevices(Guid userId)
    {
        return await GetSet()
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.BindingAt)
            .ToListAsync();
    }
}