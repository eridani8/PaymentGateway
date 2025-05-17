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
    public Result Pong(PingDto dto)
    {
        if (string.IsNullOrEmpty(dto.Model)) return Result.Failure(DeviceErrors.ModelIsEmpty);

        var now = DateTime.Now;
        if (devices.All.TryGetValue(dto.Id, out var deviceState))
        {
            deviceState.Timestamp = now;
        }
        else
        {
            var deviceDto = mapper.Map<DeviceDto>(dto);
            devices.All.TryAdd(dto.Id, deviceDto);
            logger.LogInformation("Устройство онлайн {DeviceId}", dto.Id);
        }

        return Result.Success();
    }

    public List<DeviceDto> GetAvailableDevices()
    {
        return devices.All.Values.ToList();
    }
}

public class OnlineDevices
{
    public ConcurrentDictionary<Guid, DeviceDto> All { get; } = new();
}