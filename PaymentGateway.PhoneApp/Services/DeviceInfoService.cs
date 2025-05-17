using Android.OS;
using PaymentGateway.PhoneApp.Interfaces;

namespace PaymentGateway.PhoneApp.Services;

public class DeviceInfoService : IDeviceInfoService
{
    public string GetDeviceModel()
    {
        return Build.Model ?? "";
    }
}