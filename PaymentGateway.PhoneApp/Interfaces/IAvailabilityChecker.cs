namespace PaymentGateway.PhoneApp.Interfaces;

public interface IAvailabilityChecker
{
    bool State { get; }
    Task CheckAvailable();
    Task ShowOrHideUnavailableModal();
    Task BackgroundCheckAsync();
}