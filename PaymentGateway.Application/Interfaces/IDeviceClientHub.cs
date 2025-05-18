using PaymentGateway.Shared.DTOs.Device;

namespace PaymentGateway.Application.Interfaces;

public interface IDeviceClientHub
{
    Task<DeviceDto?> GetDeviceInfoAsync();
}