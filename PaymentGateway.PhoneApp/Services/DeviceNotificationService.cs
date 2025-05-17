using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.Shared.DTOs.Device;
using PaymentGateway.Shared.Services;
using PaymentGateway.Shared.Types;

namespace PaymentGateway.PhoneApp.Services;

public class DeviceNotificationService(
    IOptions<ApiSettings> settings,
    ILogger<DeviceNotificationService> logger,
    IDeviceInfoService deviceInfoService,
    IAlertService alertService,
    LiteContext context)
    : BaseSignalRService($"{settings.Value.BaseAddress}/deviceHub", logger), IDeviceNotificationService
{

    public async Task InitializeAsync()
    {
        if (context.DeviceId == Guid.Empty) return;
        await InitializeConnectionAsync();
    }

    protected override async Task HandleConnectionError(Exception error)
    {
        if (error.Message.Contains("Unauthorized") || error.Message.Contains("401"))
        {
            await Task.CompletedTask;
        }
    }

    protected override async Task HandleInitializationError(Exception error)
    {
        if (error.Message.Contains("Unauthorized") || error.Message.Contains("401"))
        {
            await Task.CompletedTask;
        }
    }
} 