using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.Input;

namespace PaymentGateway.PhoneApp.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    [RelayCommand]
    private async Task ShowToast()
    {
        try
        {
            IsBusy = true;
            await Toast.Make("hello").Show();
            await Task.Delay(2000);
        }
        finally
        {
            IsBusy = false;
        }
    }
}