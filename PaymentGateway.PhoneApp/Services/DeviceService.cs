using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.Shared.Services;
using PaymentGateway.Shared.Types;
using PaymentGateway.Shared.DTOs.Device;

namespace PaymentGateway.PhoneApp.Services;

public class DeviceService(
    IOptions<ApiSettings> settings,
    ILogger<DeviceService> logger,
    IDeviceInfoService infoService,
    LiteContext context) : BaseSignalRService(settings, logger)
{
    protected override async Task ConfigureHubConnectionAsync()
    {
        await base.ConfigureHubConnectionAsync();

        HubConnection?.On("RequestDeviceRegistration", async () =>
        {
            var deviceInfo = new DeviceDto()
            {
                Id = context.DeviceId,
                DeviceData = infoService.GetDeviceData()
            };
            await HubConnection.InvokeAsync("RegisterDevice", deviceInfo);
        });
    }
}