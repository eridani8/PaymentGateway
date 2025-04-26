using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.Input;
using PaymentGateway.PhoneApp.Pages;

namespace PaymentGateway.PhoneApp.ViewModels;

public partial class MainViewModel(ServiceUnavailableViewModel serviceUnavailableViewModel) : BaseViewModel
{
    [RelayCommand]
    private async Task ShowToast()
    {
        try
        {
            IsBusy = true;
            await Toast.Make("hello").Show();
            await Task.Delay(2000);

            await Shell.Current.Navigation.PushModalAsync(new ServiceUnavailablePage(serviceUnavailableViewModel), true);
        }
        finally
        {
            IsBusy = false;
        }
    }
}