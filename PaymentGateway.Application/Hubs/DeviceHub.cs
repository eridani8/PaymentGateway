using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Shared.DTOs.Device;

namespace PaymentGateway.Application.Hubs;

public class DeviceHub(ILogger<DeviceHub> logger) : Hub<IDeviceClientHub>
{
    public static readonly ConcurrentDictionary<string, DeviceDto> ConnectedDevices = new();

    public override async Task OnConnectedAsync()
    {
        try
        {
            var context = Context;
            ConnectedDevices[Context.ConnectionId] = new DeviceDto();
            await base.OnConnectedAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при подключении устройства: {ConnectionId}", Context.ConnectionId);
        }
    }

    public Task Ping()
    {
        return Task.CompletedTask;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            if (ConnectedDevices.TryRemove(Context.ConnectionId, out var device))
            {
                
            }
            
            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при отключении устройства: {ConnectionId}", Context.ConnectionId);
        }
    }
}