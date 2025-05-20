using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.Shared.Constants;
using PaymentGateway.Shared.Services;
using PaymentGateway.Shared.Types;
using PaymentGateway.Shared.DTOs.Device;

namespace PaymentGateway.PhoneApp.Services;

public class DeviceService(
    IOptions<ApiSettings> settings,
    ILogger<DeviceService> logger,
    IDeviceInfoService infoService) : BaseSignalRService(settings, logger)
{
    public Action? UpdateDelegate;

    public async Task Stop()
    {
        try
        {
            await StopAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка отключения");
        }
    }
    
    public void OnConnectionStateChanged(object? sender, bool e)
    {
        UpdateDelegate?.Invoke();
        logger.LogDebug("Состояние сервиса изменилось на {State}", e);
    }
    
    protected override async Task ConfigureHubConnectionAsync()
    {
        await base.ConfigureHubConnectionAsync();

        HubConnection?.On(SignalREvents.DeviceApp.RequestDeviceRegistration, async () =>
        {
            var deviceInfo = new DeviceDto()
            {
                Id = infoService.DeviceId,
                DeviceData = infoService.GetDeviceData()
            };
            await HubConnection.InvokeAsync(SignalREvents.DeviceApp.RegisterDevice, deviceInfo);
        });
    }
}