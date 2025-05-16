using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.Shared.Types;

namespace PaymentGateway.PhoneApp.Services;

public class DeviceService(
    IHttpClientFactory clientFactory,
    ILogger<DeviceService> logger,
    JsonSerializerOptions jsonSerializerOptions,
    LiteContext context)
    : ServiceBase(clientFactory, logger, jsonSerializerOptions), IDeviceService
{
    private const string apiEndpoint = "api/v1/device";
    public bool State { get; private set; }

    public async Task SendPing()
    {
        try
        {
            if (context.DeviceId == Guid.Empty) return;
            var response = await PostRequest($"{apiEndpoint}/pong?code={context.DeviceId}");
            State = response.Code == HttpStatusCode.OK;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка проверки доступности");
        }
    }
}