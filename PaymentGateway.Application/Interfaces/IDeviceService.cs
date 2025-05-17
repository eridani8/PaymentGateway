using PaymentGateway.Application.Results;
using PaymentGateway.Shared.DTOs.Device;

namespace PaymentGateway.Application.Interfaces;

public interface IDeviceService
{
    Result<PongDto> Pong(PingDto dto);
    List<DeviceDto> GetOnlineDevices();
}