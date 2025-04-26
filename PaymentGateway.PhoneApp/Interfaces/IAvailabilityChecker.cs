namespace PaymentGateway.PhoneApp.Interfaces;

public interface IAvailabilityChecker
{
    bool State { get; }
    Task CheckAvailable(CancellationToken token = default);
}