using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Shared.DTOs.Device;

namespace PaymentGateway.Application.Hubs;

public class DeviceHub(ILogger<DeviceHub> logger) : Hub<IDeviceClientHub>
{
    public static ConcurrentDictionary<string, DeviceDto> ConnectedDevices { get; } = new();
    private static readonly TimeSpan RegistrationTimeout = TimeSpan.FromSeconds(5);

    public override async Task OnConnectedAsync()
    {
        try
        {
            await base.OnConnectedAsync();
            
            _ = Task.Delay(RegistrationTimeout).ContinueWith(_ =>
            {
                if (ConnectedDevices.ContainsKey(Context.ConnectionId)) return;
                logger.LogWarning("Устройство не зарегистрировалось в течение {Timeout} секунд. Отключение клиента: {ConnectionId}", 
                    RegistrationTimeout.TotalSeconds, Context.ConnectionId);
                Context.Abort();
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при подключении устройства: {ConnectionId}", Context.ConnectionId);
            Context.Abort();
        }
    }

    public Task RegisterDevice(DeviceDto? device)
    {
        if (device == null)
        {
            logger.LogWarning("Устройство не предоставило информацию о себе. Отключение клиента: {ConnectionId}", Context.ConnectionId);
            Context.Abort();
            return Task.CompletedTask;
        }

        logger.LogInformation("Устройство онлайн: {Device}", device.DeviceData);
        ConnectedDevices[Context.ConnectionId] = device;
        return Task.CompletedTask;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            if (ConnectedDevices.TryRemove(Context.ConnectionId, out var device))
            {
                logger.LogInformation("Устройство оффлайн: {Device}", device.DeviceData);
            }
            
            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при отключении устройства: {ConnectionId}", Context.ConnectionId);
        }
    }
}