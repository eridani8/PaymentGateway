namespace PaymentGateway.PhoneApp.Interfaces;

public interface IDeviceService
{
    bool State { get; }
    Task SendPing();
}