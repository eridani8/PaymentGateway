using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Shared.DTOs.Device;
using Microsoft.AspNetCore.Authorization;

namespace PaymentGateway.Application.Hubs;

[Authorize]
public class DeviceHub(ILogger<DeviceHub> logger) : Hub<IDeviceClientHub>
{
    public static ConcurrentDictionary<string, DeviceDto> ConnectedDevices { get; } = new();
    private static readonly TimeSpan RegistrationTimeout = TimeSpan.FromSeconds(5);

    public override async Task OnConnectedAsync()
    {
        try
        {
            await base.OnConnectedAsync();
            var context = Context;
            var connectionId = Context.ConnectionId;
            
            await Clients.Caller.RequestDeviceRegistration();
            
            _ = Task.Delay(RegistrationTimeout).ContinueWith(_ =>
            {
                if (ConnectedDevices.ContainsKey(connectionId)) return;
                logger.LogWarning("Устройство не зарегистрировалось в течение {Timeout} секунд. Отключение клиента: {ConnectionId}", 
                    RegistrationTimeout.TotalSeconds, connectionId);
                context.Abort();
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при подключении устройства");
        }
    }

    public Task RegisterDevice(DeviceDto? device)
    {
        if (device == null || string.IsNullOrEmpty(device.DeviceData) || device.Id == Guid.Empty)
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