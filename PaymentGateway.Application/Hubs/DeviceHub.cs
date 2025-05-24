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
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _registrationTimeouts = new();
    private static readonly TimeSpan RegistrationTimeout = TimeSpan.FromSeconds(7);

    public static DeviceDto? DeviceByIdAndUserId(Guid id, Guid userId)
    {
        return Devices.Values
            .FirstOrDefault(d =>
                d.UserId == userId &&
                d.Id == id);
    }
    
    public static List<DeviceDto> AvailableDevicesByUserId(Guid userId)
    {
        return Devices.Values
            .Where(d => d.UserId == userId && d.BindingAt == DateTime.MinValue)
            .ToList();
    }

    public static IEnumerable<DeviceDto> UserDevices(Guid userId)
    {
        return Devices.Values
            .Where(d => d.UserId == userId);
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var context = Context;
            var connectionId = Context.ConnectionId;

            await Clients.Caller.RequestDeviceRegistration();

            var cancellationTokenSource = new CancellationTokenSource();
            _registrationTimeouts[connectionId] = cancellationTokenSource;

            _ = Task.Delay(RegistrationTimeout, cancellationTokenSource.Token).ContinueWith(task =>
            {
                if (task.IsCanceled) return;

                if (Devices.Values.Any(d => d.ConnectionId == connectionId)) return;

                logger.LogWarning(
                    "Устройство не зарегистрировалось в течение {Timeout} секунд. Отключение клиента: {ConnectionId}",
                    RegistrationTimeout.TotalSeconds, connectionId);
                context.Abort();
                _registrationTimeouts.TryRemove(connectionId, out _);
            }, cancellationTokenSource.Token);

            await base.OnConnectedAsync();
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

        var connectionId = Context.ConnectionId;

        if (Devices.TryGetValue(deviceDto.Id, out var existingDevice))
        {
            existingDevice.State = true;
            existingDevice.ConnectionId = connectionId;

            CancelRegistrationTimeout(connectionId);

            await notificationService.NotifyDeviceUpdated(existingDevice);
            logger.LogInformation("Устройство подключено: {DeviceName} (ID: {DeviceId})", existingDevice.DeviceName,
                existingDevice.Id);
        }
        else
        {
            var userIdClaim = Context.User?.FindFirst("i")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                logger.LogWarning("Не удалось получить ID пользователя из контекста для устройства {DeviceId}",
                    deviceDto.Id);
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
            deviceDto.ConnectionId = connectionId;
            deviceDto.State = true;

            if (!Devices.TryAdd(deviceDto.Id, deviceDto))
            {
                logger.LogError("Не удалось добавить устройство: {@Device}", deviceDto);
                Context.Abort();
                return;
            }

            CancelRegistrationTimeout(connectionId);

            await notificationService.NotifyDeviceUpdated(deviceDto);
            logger.LogInformation("Новое устройство зарегистрировано: {DeviceName} (ID: {DeviceId})",
                deviceDto.DeviceName, deviceDto.Id);
        }
    }

    private void CancelRegistrationTimeout(string connectionId)
    {
        if (!_registrationTimeouts.TryRemove(connectionId, out var cancellationTokenSource)) return;
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;

        CancelRegistrationTimeout(connectionId);

        var device = Devices.Values.FirstOrDefault(d => d.ConnectionId == connectionId);
        if (device is not null)
        {
            device.State = false;
            device.ConnectionId = null;
            
            await notificationService.NotifyDeviceUpdated(device);
            logger.LogInformation("Устройство отключено: {DeviceName} (ID: {DeviceId})", device.DeviceName, device.Id);
        }

        await base.OnDisconnectedAsync(exception);
    }
}