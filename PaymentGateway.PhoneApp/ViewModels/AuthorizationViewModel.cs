using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.PhoneApp.Services;

namespace PaymentGateway.PhoneApp.ViewModels;

public partial class AuthorizationViewModel(
    IAlertService alertService,
    ILogger<AuthorizationViewModel> logger,
    DeviceService deviceService)
    : ObservableObject
{
    [ObservableProperty] private DeviceService _deviceService = deviceService;

    [RelayCommand]
    private async Task Logout()
    {
        await DeviceService.Stop();
        ClearToken();
    }

    [RelayCommand]
    private async Task Authorize()
    {
        if (string.IsNullOrEmpty(DeviceService.AccessToken)) return;
        
        try
        {
            await DeviceService.InitializeAsync();
            if (!await DeviceService.WaitConnection(TimeSpan.FromSeconds(7)))
            {
                await FailureConnection();
                return;
            }
            DeviceService.SaveToken();
            await alertService.ShowAlertAsync("Успех", "Авторизация выполнена", "OK");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка авторизации");
            await FailureConnection();
        }
    }

    private async Task FailureConnection()
    {
        ClearToken();
        await alertService.ShowAlertAsync("Ошибка", "Не удалось выполнить авторизацию", "OK");
    }

    private void ClearToken()
    {
        DeviceService.AccessToken = string.Empty;
    }
}