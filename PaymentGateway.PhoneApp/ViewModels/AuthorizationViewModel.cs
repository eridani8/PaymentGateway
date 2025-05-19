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
            await deviceService.InitializeAsync();
            deviceInfoService.SaveToken();
            await alertService.ShowAlertAsync("Успех", "Авторизация выполнена", "OK");
        }
        catch (Exception e)
        {
            deviceInfoService.Token = string.Empty;
            Token = string.Empty;
            logger.LogError(e, "Ошибка авторизации");
            await alertService.ShowAlertAsync("Ошибка", "Не удалось выполнить авторизацию", "OK");
        }
    }
}