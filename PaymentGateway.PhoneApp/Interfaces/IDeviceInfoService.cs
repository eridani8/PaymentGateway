namespace PaymentGateway.PhoneApp.Interfaces;

public interface IDeviceInfoService
{
    Guid DeviceId { get; }
    string Token { get; set; }

    void SaveToken();
    string GetDeviceData();
}