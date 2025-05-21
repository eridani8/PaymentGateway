using PaymentGateway.Shared.DTOs.Device;

namespace PaymentGateway.Application.Interfaces;

public interface IDeviceClientHub
{
    Task RequestDeviceRegistration();
    Task DeviceConnected(DeviceDto device);
    Task DeviceDisconnected(DeviceDto device);
}