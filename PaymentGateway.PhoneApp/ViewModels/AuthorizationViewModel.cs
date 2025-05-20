using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.PhoneApp.Services;

namespace PaymentGateway.PhoneApp.ViewModels;

public partial class AuthorizationViewModel(
    IAlertService alertService,
    ILogger<AuthorizationViewModel> logger,
    DeviceService deviceService,
    IDeviceInfoService deviceInfoService)
    : ObservableObject
{
    [ObservableProperty] private string? _token;

    [RelayCommand]
    private async Task Authorize()
    {
        if (string.IsNullOrEmpty(Token)) return;
        
        try
        {
            deviceInfoService.Token = Token;
            deviceService.AccessToken = Token;
            await deviceService.InitializeAsync();
            if (!await deviceService.WaitConnection(TimeSpan.FromSeconds(10)))
            {
                await FailureConnection();
                return;
            }
            deviceInfoService.SaveToken();
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
        deviceInfoService.Token = string.Empty;
        deviceService.AccessToken = string.Empty;
        Token = string.Empty;
        await alertService.ShowAlertAsync("Ошибка", "Не удалось выполнить авторизацию", "OK");
    }
}