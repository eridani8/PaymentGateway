namespace PaymentGateway.PhoneApp.Interfaces;

public interface IServiceAvailabilityChecker
{
    Task<bool> CheckAvailable();
}