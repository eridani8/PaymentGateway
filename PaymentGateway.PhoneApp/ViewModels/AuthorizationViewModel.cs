using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.PhoneApp.Services;
using Android.Content;
using PaymentGateway.PhoneApp.Pages;

namespace PaymentGateway.PhoneApp.ViewModels;

public partial class AuthorizationViewModel(
    ILogger<AuthorizationViewModel> logger,
    DeviceService deviceService)
    : ObservableObject
{
    [ObservableProperty] private DeviceService _deviceService = deviceService;

    [RelayCommand]
    private async Task Logout()
    {
        await DeviceService.Logout();
    }

    [RelayCommand]
    private async Task Authorize()
    {
        await DeviceService.Authorize();
    }

    [RelayCommand]
    private async Task ScanQr()
    {
        DeviceService.ClearToken();
            
        var scannerPage = new QrScannerPage();
        scannerPage.OnQrCodeScanned += async (_, code) =>
        {
            DeviceService.AccessToken = code;
            if (await DeviceService.Authorize())
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.Navigation.PopAsync();
                });
            }
            else
            {
                await Task.Delay(3000);
            }
        };

        await Shell.Current.Navigation.PushAsync(scannerPage);
    }
}