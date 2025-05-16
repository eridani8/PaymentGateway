using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;

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
            logger.LogInformation("Updated device state: {DeviceId}", code);
        }
        else
        {
            deviceStates.TryAdd(code, new DeviceState()
            {
                Timestamp = now
            });
            logger.LogInformation("Added device state: {DeviceId}", code);
        }

        return Task.CompletedTask;
    }
}

public class DeviceState
{
    public DateTime Timestamp { get; set; }
}