using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Shared.DTOs.Device;
using Microsoft.AspNetCore.Authorization;

namespace PaymentGateway.Application.Hubs;

[Authorize(AuthenticationSchemes = "Device")]
public class DeviceHub(
    ILogger<DeviceHub> logger,
    INotificationService notificationService) : Hub<IDeviceClientHub>
{
    public static ConcurrentDictionary<string, DeviceDto> ConnectedDevices { get; } = new();
    private static readonly TimeSpan RegistrationTimeout = TimeSpan.FromSeconds(7);

    public override async Task OnConnectedAsync()
    {
        try
        {
            await base.OnConnectedAsync();
            
            var context = Context;
            var connectionId = Context.ConnectionId;
            
            var userId = context.User?.FindFirst("i")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                context.Abort();
                return;
            }
            
            var userGuid = Guid.Parse(userId);
            
            await Clients.Caller.RequestDeviceRegistration();
            
            _ = Task.Delay(RegistrationTimeout).ContinueWith(_ =>
            {
                if (!ConnectedDevices.TryGetValue(connectionId, out var device))
                {
                    logger.LogWarning(
                        "Устройство не зарегистрировалось в течение {Timeout} секунд. Отключение клиента: {ConnectionId}",
                        RegistrationTimeout.TotalSeconds, connectionId);
                    context.Abort();
                    return;
                }
                
                device.UserId = userGuid;
                notificationService.DeviceConnected(device);
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
                await notificationService.DeviceDisconnected(device);
            }
            
            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при отключении устройства: {ConnectionId}", Context.ConnectionId);
        }
    }
}