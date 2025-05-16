using System.Net;
using System.Text.Json;
using LiteDB;
using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.PhoneApp.Types;
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
    public Guid DeviceId { get; private set; } = Guid.Empty;

    public async Task SendPing()
    {
        try
        {
            if (DeviceId == Guid.Empty)
            {
                if (context.KeyValues.FindOne(k => k.Key == "DeviceId") is not { } keyValue)
                {
                    keyValue = new KeyValue()
                    {
                        Id = ObjectId.NewObjectId(),
                        Key = "DeviceId",
                        Value = Guid.CreateVersion7()
                    };
                    context.KeyValues.Insert(keyValue);
                }
            
                if (keyValue.Value is Guid guid)
                {
                    DeviceId = guid;
                }
                else
                {
                    DeviceId = Guid.Empty;
                }
            }
            
            if (DeviceId == Guid.Empty)
            {
                logger.LogError("Ошибка назначения DeviceId");
                return;
            }
            
            var response = await PostRequest($"{apiEndpoint}/pong", new PingDto()
            {
                Id = DeviceId,
                Model = deviceInfoService.GetDeviceModel()
            });
            State = response.Code == HttpStatusCode.OK;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка проверки доступности");
        }
    }
}