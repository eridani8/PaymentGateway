using System.Collections.Concurrent;
using FluentValidation;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Types;
using PaymentGateway.Shared.DTOs.Device;

namespace PaymentGateway.Application.Services;

public class DeviceService(
    ConcurrentDictionary<Guid, DeviceState> deviceStates,
    ILogger<DeviceService> logger)
    : IDeviceService
{
    public Task Pong(PingDto dto)
    {
        var now = DateTime.Now;
        if (deviceStates.TryGetValue(dto.DeviceId, out var deviceState))
        {
            deviceState.Timestamp = now;
        }
        else
        {
            deviceStates.TryAdd(dto.DeviceId, new DeviceState()
            {
                Timestamp = now
            });
            logger.LogInformation("added device state: {DeviceId}", dto.DeviceId);
        }

        return Task.CompletedTask;
    }
}