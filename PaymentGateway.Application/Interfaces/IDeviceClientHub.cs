using PaymentGateway.Shared.DTOs.Device;
using PaymentGateway.Shared.DTOs.Transaction;

namespace PaymentGateway.Application.Interfaces;

public interface IDeviceClientHub
{
    Task RequestDeviceRegistration();
    Task DeviceConnected(DeviceDto device);
    Task DeviceDisconnected(DeviceDto device);
    Task RegisterRequisite(Guid? requisiteId);
    Task TransactionReceived(TransactionCreateDto transaction);
}