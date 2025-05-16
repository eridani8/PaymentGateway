using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.PhoneApp.Services;

namespace PaymentGateway.PhoneApp.ViewModels;

public partial class DeviceIdViewModel(LiteContext context) : BaseViewModel
{
    [ObservableProperty] private string _deviceId = context.DeviceId.ToString();

    [RelayCommand]
    private async Task CopyToClipboard()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            await Clipboard.SetTextAsync(DeviceId);
            await Toast.Make("Скопировано").Show();
        }
        finally
        {
            IsBusy = false;
        }
    }
} 