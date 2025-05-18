namespace PaymentGateway.Application.Interfaces;

public interface IDeviceClientHub
{
    Task RequestDeviceRegistration();
}