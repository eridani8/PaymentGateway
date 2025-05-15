using PaymentGateway.Core.Entities;

namespace PaymentGateway.Infrastructure.Interfaces;

public interface IDeviceRepository : IRepositoryBase<DeviceEntity>
{
    Task<List<DeviceEntity>> GetAllDevices();
    Task<List<DeviceEntity>> GetUserDevices(Guid userId);
}