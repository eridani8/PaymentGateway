using System.Net;
using System.Text.Json;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.Shared.DTOs.Device;
using PaymentGateway.Shared.Types;

namespace PaymentGateway.PhoneApp.Services;

public class DeviceService(
    IHttpClientFactory clientFactory,
    ILogger<DeviceService> logger,
    JsonSerializerOptions jsonSerializerOptions,
    LiteContext context,
    IDeviceInfoService deviceInfoService)
    : ServiceBase(clientFactory, logger, jsonSerializerOptions), IDeviceService
{
    private const string apiEndpoint = "api/device";
    public bool State { get; private set; }

    public async Task SendPing()
    {
        try
        {
            if (context.DeviceId == Guid.Empty) return;

            var response = await PostRequest<PongDto>($"{apiEndpoint}/pong", new PingDto()
            {
                Id = context.DeviceId,
                Model = deviceInfoService.GetDeviceModel()
            });

            if (response != null)
            {
                State = true;
                
                switch (response.Action)
                {
                    case DeviceAction.ConfirmBinding:
                        MainThread.BeginInvokeOnMainThread(async void () =>
                        {
                            try
                            {
                                await Toast.Make("test").Show();
                            }
                            catch
                            {
                                // ignored
                            }
                        });
                        break;
                    case DeviceAction.None:
                    default:
                        break;
                }
                
                logger.LogInformation("pong {Action}", response.Action.ToString());
            }
            else
            {
                State = false;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка проверки доступности");
        }
    }
}