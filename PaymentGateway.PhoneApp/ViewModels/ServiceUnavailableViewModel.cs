using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.Shared.Services;

namespace PaymentGateway.PhoneApp.ViewModels;

public partial class ServiceUnavailableViewModel(
    BaseSignalRService signalRService,
    ILogger<ServiceUnavailableViewModel> logger) : BaseViewModel
{
    [RelayCommand]
    private async Task CheckConnection()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            // await deviceService.SendPing(); // TODO
            // await deviceService.ShowOrHideUnavailableModal(); // TODO
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