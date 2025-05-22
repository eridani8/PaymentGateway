using PaymentGateway.Shared.DTOs.Device;

namespace PaymentGateway.Web.Interfaces;

public interface IDeviceService
{
    Task<DeviceTokenDto?> GenerateDeviceToken();
    Task<List<DeviceDto>> GetOnlineDevices();
    Task<List<DeviceDto>> GetUserOnlineDevices();
    Task<List<DeviceDto>> GetDevicesByUserId(Guid userId);
}