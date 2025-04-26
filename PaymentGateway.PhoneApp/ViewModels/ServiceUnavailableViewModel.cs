using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;

namespace PaymentGateway.PhoneApp.ViewModels;

public partial class ServiceUnavailableViewModel(
    IAvailabilityChecker checker,
    ILogger<ServiceUnavailableViewModel> logger) : BaseViewModel
{
    [RelayCommand]
    private async Task CheckConnection()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var result = await checker.CheckAvailable();
            if (result)
            {
                await Shell.Current.Navigation.PopModalAsync(true);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка проверки доступности сервиса");
        }
        finally
        {
            IsBusy = false;
        }
    }
}