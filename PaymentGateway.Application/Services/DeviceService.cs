using System.Collections.Concurrent;
using FluentValidation;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Types;
using PaymentGateway.Shared.DTOs.Device;

namespace PaymentGateway.Application.Services;

public class DeviceService(
    AvailableDevices devices,
    ILogger<DeviceService> logger)
    : IDeviceService
{
    public Task Pong(PingDto dto)
    {
        var now = DateTime.Now;
        if (devices.Devices.TryGetValue(dto.DeviceId, out var deviceState))
        {
            deviceState.Timestamp = now;
        }
        else
        {
            devices.Devices.TryAdd(dto.DeviceId, new DeviceState()
            {
                Timestamp = now
            });
            logger.LogInformation("Устройство онлайн {DeviceId}", dto.DeviceId);
        }

        return Task.CompletedTask;
    }
}