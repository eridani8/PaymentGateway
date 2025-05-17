using System.Collections.Concurrent;
using AutoMapper;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Results;
using PaymentGateway.Shared.DTOs.Device;

namespace PaymentGateway.Application.Services;

public class DeviceService(
    OnlineDevices devices,
    ILogger<DeviceService> logger,
    IMapper mapper)
    : IDeviceService
{
    public Result<PongDto> Pong(PingDto dto)
    {
        if (string.IsNullOrEmpty(dto.Model)) return Result.Failure<PongDto>(DeviceErrors.ModelIsEmpty);

        if (devices.All.TryGetValue(dto.Id, out var device))
        {
            device.Timestamp = DateTime.Now;
        }
        else
        {
            device = mapper.Map<DeviceDto>(dto);
            devices.All.TryAdd(dto.Id, device);
            logger.LogInformation("Устройство онлайн {DeviceId}", dto.Id);
        }

        var pong = mapper.Map<PongDto>(device);

        if (device.Action != DeviceAction.None)
        {
            device.Action = DeviceAction.None;
        }
        
        return Result.Success(pong);
    }

    public List<DeviceDto> GetOnlineDevices()
    {
        return devices.All.Values.ToList();
    }
}

public class OnlineDevices
{
    public ConcurrentDictionary<Guid, DeviceDto> All { get; } = new();
}