using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Infrastructure.Interfaces;

namespace PaymentGateway.Infrastructure.Repositories;

public class DeviceRepository(
    AppDbContext context)
    : RepositoryBase<DeviceEntity>(context), IDeviceRepository
{
    public async Task<DeviceEntity?> GetDeviceById(Guid id)
    {
        return await GetSet()
            .Include(d => d.User)
            .Include(d => d.Requisite)
            .OrderByDescending(d => d.Id)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<List<DeviceEntity>> GetAllDevices()
    {
        return await GetSet()
            .Include(d => d.User)
            .AsNoTracking()
            .OrderByDescending(d => d.Id)
            .ToListAsync();
    }

    public async Task<List<DeviceEntity>> GetUserDevices(Guid userId)
    {
        return await GetSet()
            .Include(d => d.User)
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.Id)
            .ToListAsync();
    }
}