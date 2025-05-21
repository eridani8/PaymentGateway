using PaymentGateway.Shared.DTOs.Device;

namespace PaymentGateway.Web.Interfaces;

public interface IDeviceService
{
    Task<DeviceTokenDto?> GenerateDeviceToken();
    Task<List<DeviceDto>> GetAllOnlineDevices();
    Task<List<DeviceDto>> GetUserOnlineDevices();
}