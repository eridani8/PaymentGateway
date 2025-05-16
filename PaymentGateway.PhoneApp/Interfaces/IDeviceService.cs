namespace PaymentGateway.PhoneApp.Interfaces;

public interface IDeviceService
{
    Guid DeviceId { get; }
    bool State { get; }
    Task SendPing();
}