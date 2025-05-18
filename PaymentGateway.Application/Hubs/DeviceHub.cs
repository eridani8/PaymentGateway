using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;

namespace PaymentGateway.Application.Hubs;

public class DeviceHub(ILogger<DeviceHub> logger) : Hub<IDeviceClientHub>
{
    private static readonly ConcurrentDictionary<string, object> ConnectedDevices = new();
}