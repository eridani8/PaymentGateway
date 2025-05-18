using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Shared.DTOs.Device;

namespace PaymentGateway.Application.Hubs;

public class DeviceHub(ILogger<DeviceHub> logger) : Hub<IDeviceClientHub>
{
    public static ConcurrentDictionary<string, DeviceDto> ConnectedDevices { get; } = new();

    public override async Task OnConnectedAsync()
    {
        try
        {
            var device = await Clients.Caller.GetDeviceInfoAsync();
            if (device == null)
            {
                logger.LogWarning("Устройство не предоставило информацию о себе. Отключение клиента: {ConnectionId}", Context.ConnectionId);
                Context.Abort();
                return;
            }

            logger.LogInformation("Устройство онлайн: {@Device}", device);
            ConnectedDevices[Context.ConnectionId] = device;
            await base.OnConnectedAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при подключении устройства: {ConnectionId}", Context.ConnectionId);
            Context.Abort();
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
                logger.LogInformation("Устройство оффлайн: {@Device}", device);
            }
            
            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при отключении устройства: {ConnectionId}", Context.ConnectionId);
        }
    }
}