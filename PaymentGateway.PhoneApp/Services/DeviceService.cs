using System.Text.Json;
using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.Shared.DTOs.Device;
using PaymentGateway.Shared.Types;

namespace PaymentGateway.PhoneApp.Services;

public class DeviceService(
    IHttpClientFactory clientFactory,
    ILogger<DeviceService> logger,
    JsonSerializerOptions jsonSerializerOptions)
    : ServiceBase(clientFactory, logger, jsonSerializerOptions), IDeviceService
{
    private const string apiEndpoint = "api/v1/device";

    public async Task SendDeviceId(Guid deviceId)
    {
        try
        {
            await PostRequest($"{apiEndpoint}/reception-code", new ReceptionCodeDto()
            {
                DeviceCode = deviceId
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка отправки Id устройства");
        }
    }
}