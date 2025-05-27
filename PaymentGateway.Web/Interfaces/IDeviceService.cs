using PaymentGateway.Shared.DTOs.Device;
using PaymentGateway.Shared.Types;

namespace PaymentGateway.Web.Interfaces;

public interface IDeviceService
{
    Task<DeviceTokenDto?> GenerateDeviceToken();
    Task<Response> DeleteDevice(Guid id);
    Task<List<DeviceDto>> GetDevices();
    Task<List<DeviceDto>> GetUserDevices(bool onlyAvailable = false, bool onlyOnline = false);
    Task<List<DeviceDto>> GetDevicesByUserId(Guid userId);
}