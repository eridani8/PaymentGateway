using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.PhoneApp.Services;

namespace PaymentGateway.PhoneApp.ViewModels;

public partial class DeviceIdViewModel(IDeviceInfoService infoService) : BaseViewModel
{
    [ObservableProperty] private string _deviceId = infoService.DeviceId.ToString();

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