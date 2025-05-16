using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Types;

namespace PaymentGateway.Application.Services;

public class DeviceService(ConcurrentDictionary<Guid, DeviceState> deviceStates, ILogger<DeviceService> logger)
    : IDeviceService
{
    public Task Pong(Guid code)
    {
        var now = DateTime.Now;
        if (deviceStates.TryGetValue(code, out var deviceState))
        {
            deviceState.Timestamp = now;
        }
        else
        {
            deviceStates.TryAdd(code, new DeviceState()
            {
                Timestamp = now
            });
            logger.LogInformation("added device state: {DeviceId}", code);
        }

        return Task.CompletedTask;
    }
}