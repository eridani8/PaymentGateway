using System.Collections.Concurrent;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Shared.DTOs.Device;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using PaymentGateway.Core.Entities;
using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Application.Hubs;

[Authorize(AuthenticationSchemes = "Device")]
public class DeviceHub(
    ILogger<DeviceHub> logger,
    INotificationService notificationService,
    UserManager<UserEntity> userManager,
    IMapper mapper) : Hub<IDeviceClientHub>
{
    public static ConcurrentDictionary<Guid, DeviceDto> Devices { get; } = new();
    private static readonly TimeSpan RegistrationTimeout = TimeSpan.FromSeconds(7);

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
                if (Devices.Values.Any(d => d.ConnectionId == connectionId)) return;
                logger.LogWarning(
                    "Устройство не зарегистрировалось в течение {Timeout} секунд. Отключение клиента: {ConnectionId}",
                    RegistrationTimeout.TotalSeconds, connectionId);
                context.Abort();
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при подключении устройства");
        }
    }

    public async Task RegisterDevice(DeviceDto? deviceDto)
    {
        if (deviceDto is null || string.IsNullOrWhiteSpace(deviceDto.DeviceName) || deviceDto.Id == Guid.Empty)
        {
            logger.LogWarning("Получены невалидные данные устройства: {@DeviceDto}", deviceDto);
            Context.Abort();
            return;
        }
        
        if (Devices.TryGetValue(deviceDto.Id, out var existingDevice))
        {
            existingDevice.State = true;
            existingDevice.ConnectionId = Context.ConnectionId;
            
            await notificationService.DeviceConnected(existingDevice);
            logger.LogInformation("Устройство подключено: {DeviceName} (ID: {DeviceId})", existingDevice.DeviceName, existingDevice.Id);
        }
        else
        {
            var userIdClaim = Context.User?.FindFirst("i")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                logger.LogWarning("Не удалось получить ID пользователя из контекста для устройства {DeviceId}", deviceDto.Id);
                Context.Abort();
                return;
            }
            
            var user = await userManager.FindByIdAsync(userIdClaim);
            if (user is null)
            {
                logger.LogError("Невалидный ID пользователя: {UserId}", userIdClaim);
                Context.Abort();
                return;
            }
            
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                logger.LogError("Невалидный формат ID пользователя: {UserId}", userIdClaim);
                Context.Abort();
                return;
            }
            
            deviceDto.User = mapper.Map<UserDto>(user);
            deviceDto.UserId = userId;
            deviceDto.ConnectionId = Context.ConnectionId;
            deviceDto.State = true;
            
            if (!Devices.TryAdd(deviceDto.Id, deviceDto))
            {
                logger.LogError("Не удалось добавить устройство: {@Device}", deviceDto);
                Context.Abort();
                return;
            }
            
            await notificationService.DeviceConnected(deviceDto);
            logger.LogInformation("Новое устройство зарегистрировано: {DeviceName} (ID: {DeviceId})", deviceDto.DeviceName, deviceDto.Id);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var device = Devices.Values.FirstOrDefault(d => d.ConnectionId == Context.ConnectionId);
        if (device is not null)
        {
            device.State = false;
            device.ConnectionId = null;
            
            await notificationService.DeviceDisconnected(device);
            logger.LogInformation("Устройство отключено: {DeviceName} (ID: {DeviceId})", device.DeviceName, device.Id);
        }
        else
        {
            logger.LogWarning("Соединение не найдено");
        }
            
        await base.OnDisconnectedAsync(exception);
    }
}