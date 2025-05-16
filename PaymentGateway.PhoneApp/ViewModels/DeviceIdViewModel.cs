using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PaymentGateway.PhoneApp.Interfaces;

namespace PaymentGateway.PhoneApp.ViewModels;

public partial class DeviceIdViewModel(IDeviceService deviceService) : BaseViewModel
{
    [ObservableProperty] private string _deviceId = deviceService.DeviceId.ToString();

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