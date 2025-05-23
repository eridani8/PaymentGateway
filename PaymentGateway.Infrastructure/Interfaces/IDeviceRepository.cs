using PaymentGateway.Core.Entities;

namespace PaymentGateway.Infrastructure.Interfaces;

public interface IDeviceRepository : IRepositoryBase<DeviceEntity>
{
    Task<DeviceEntity?> GetDeviceById(Guid id);
    Task<List<DeviceEntity>> GetAllDevices();
    Task<List<DeviceEntity>> GetUserDevices(Guid userId);
}