using Android.OS;
using LiteDB;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.PhoneApp.Types;

namespace PaymentGateway.PhoneApp.Services;

public class DeviceInfoService(LiteContext context) : IDeviceInfoService
{
    public Guid DeviceId { get; } = context.GetDeviceId();
    public string Token { get; set; } = context.GetToken();

    public void SaveToken()
    {
        context.KeyValues.Insert(new KeyValue()
        {
            Id = ObjectId.NewObjectId(),
            Key = LiteContext.tokenKey,
            Value = Token
        });
    }

    public string GetDeviceData()
    {
        return $"{Build.Manufacturer} {Build.Model} ({Build.VERSION.Release})";
    }
}