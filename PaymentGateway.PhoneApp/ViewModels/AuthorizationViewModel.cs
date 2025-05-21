using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.PhoneApp.Services;
using Android.Content;

namespace PaymentGateway.PhoneApp.ViewModels;

public partial class AuthorizationViewModel(
    IAlertService alertService,
    ILogger<AuthorizationViewModel> logger,
    DeviceService deviceService,
    IBackgroundServiceManager backgroundServiceManager)
    : ObservableObject
{
    [ObservableProperty] private DeviceService _deviceService = deviceService;

    [RelayCommand]
    private async Task Logout()
    {
        await DeviceService.Stop();
        ClearToken();
        DeviceService.RemoveToken();
        DeviceService.UpdateDelegate?.Invoke();
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
            DeviceService.UpdateDelegate?.Invoke();
            
            if (!backgroundServiceManager.IsRunning)
            {
                var intent = new Intent(Platform.CurrentActivity, typeof(BackgroundService));
                intent.SetAction(AndroidConstants.ActionStart);
                Platform.CurrentActivity?.StartService(intent);
            }
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