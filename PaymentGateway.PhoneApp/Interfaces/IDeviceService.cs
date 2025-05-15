namespace PaymentGateway.PhoneApp.Interfaces;

public interface IDeviceService
{
    Task SendDeviceId(Guid deviceId);
}