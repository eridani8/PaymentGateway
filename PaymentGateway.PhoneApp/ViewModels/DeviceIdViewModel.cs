using System.Globalization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteDB;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.PhoneApp.Services;
using PaymentGateway.PhoneApp.Types;

namespace PaymentGateway.PhoneApp.ViewModels;

public partial class DeviceIdViewModel(DeviceService deviceService) : BaseViewModel
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