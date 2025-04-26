using PaymentGateway.PhoneApp.Interfaces;

namespace PaymentGateway.PhoneApp.Services;

public class AlertService : IAlertService
{
    public async Task<bool> ShowConfirmationAsync(string title, string message, string accept, string cancel)
    {
        return await Shell.Current.DisplayAlert(title, message, accept, cancel);
    }
}