using PaymentGateway.Shared.DTOs.Device;

namespace PaymentGateway.Web.Interfaces;

public interface IDeviceService
{
    Task<DeviceTokenDto?> GenerateDeviceToken();
    Task<List<DeviceDto>> GetDevices();
    Task<List<DeviceDto>> GetUserDevices(bool onlyAvailable = false);
    Task<List<DeviceDto>> GetDevicesByUserId(Guid userId);
}