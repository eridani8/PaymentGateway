using System.Text.Json;
using PaymentGateway.Shared.DTOs.Device;
using PaymentGateway.Shared.Types;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class DeviceService(
    IHttpClientFactory httpClientFactory,
    ILogger<DeviceService> logger,
    JsonSerializerOptions jsonSerializerOptions)
    : ServiceBase(httpClientFactory, logger, jsonSerializerOptions), IDeviceService
{
    private const string endpoint = "api/device";
    public async Task<DeviceTokenDto?> GenerateDeviceToken()
    {
        return await PostRequest<DeviceTokenDto>($"{endpoint}/token");
    }
}