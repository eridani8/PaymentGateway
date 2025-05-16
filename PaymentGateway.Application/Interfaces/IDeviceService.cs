namespace PaymentGateway.Application.Interfaces;

public interface IDeviceService
{
    Task Pong(Guid code);
}