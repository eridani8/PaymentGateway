using Android.OS;
using PaymentGateway.PhoneApp.Interfaces;

namespace PaymentGateway.PhoneApp.Services;

public class DeviceInfoService : IDeviceInfoService
{
    public string GetDeviceData()
    {
        return $"{Build.Manufacturer} {Build.Model} ({Build.VERSION.Release})";
    }
}