using System.Security.Cryptography;
using System.Text;
using Android.Content;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentGateway.Shared.Constants;
using PaymentGateway.Shared.Services;
using PaymentGateway.Shared.Types;
using PaymentGateway.Shared.DTOs.Device;
using Android.OS;
using LiteDB;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.PhoneApp.Types;
using PaymentGateway.Shared.DTOs.Transaction;

namespace PaymentGateway.PhoneApp.Services;

public class DeviceService : BaseSignalRService
{
    private readonly ILogger<DeviceService> _logger;
    private readonly LiteContext _context;
    private readonly IAlertService _alertService;

    private Guid DeviceId { get; }

    public Action? UpdateDelegate;

    public bool IsRunning { get; private set; }

    public DeviceService(IOptions<ApiSettings> settings,
        ILogger<DeviceService> logger,
        LiteContext context,
        IAlertService alertService) : base(settings, logger)
    {
        _logger = logger;
        _context = context;
        _alertService = alertService;
        AccessToken = context.GetToken();
        DeviceId = context.GetDeviceId();
    }

    public async Task TransactionReceived(TransactionReceivedDto dto)
    {
        try
        {
            if (HubConnection is not { State: HubConnectionState.Connected }) return;
            await HubConnection.InvokeAsync(SignalREvents.DeviceApp.TransactionReceived, dto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Ошибка при отправке транзакции сервису");
        }
    }

    #region Authorization

    public async Task Logout()
    {
        await Stop();
        ClearToken();
        IsLoggedIn = false;
        UpdateDelegate?.Invoke();
    }

    public async Task<bool> Authorize()
    {
        try
        {
            if (await InitializeAsync())
            {
                if (_context.GetToken() != AccessToken)
                {
                    SaveToken();
                    UpdateDelegate?.Invoke();
                }

                if (!IsRunning)
                {
                    var intent = new Intent(Platform.CurrentActivity!, typeof(BackgroundService));
                    intent.SetAction(AndroidConstants.ActionStart);
                    Platform.CurrentActivity!.StartService(intent);
                }

                return true;
            }
            else
            {
                await FailureConnection();
                return false;
            }
        }
        catch
        {
            await FailureConnection();
            return false;
        }
        finally
        {
            UpdateDelegate?.Invoke();
        }
    }

    private async Task FailureConnection()
    {
        await Stop();

        if (IsServiceUnavailable)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await _alertService.ShowAlertAsync("Ошибка", "Сервис временно недоступен", "OK");
            });
        }
        else if (!IsLoggedIn)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await _alertService.ShowAlertAsync("Ошибка", "Токен недействителен", "OK");
            });
        }

        IsRunning = false;
        ClearToken();
    }

    public void ClearToken()
    {
        RemoveToken();
        AccessToken = string.Empty;
    }

    private void SaveToken()
    {
        _context.KeyValues.Insert(new KeyValue()
        {
            Id = ObjectId.NewObjectId(),
            Key = LiteContext.tokenKey,
            Value = AccessToken
        });
    }

    private void RemoveToken()
    {
        if (_context.KeyValues.FindOne(e => e.Key == LiteContext.tokenKey) is { } keyValue)
        {
            _context.KeyValues.Delete(keyValue.Id);
        }
    }

    public async Task Stop()
    {
        try
        {
            IsInitializing = false;
            UpdateDelegate?.Invoke();
            await StopAsync();
            IsRunning = false;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Ошибка отключения");
        }
        finally
        {
            UpdateDelegate?.Invoke();
        }
    }

    public override async Task<bool> InitializeAsync()
    {
        IsInitializing = true;
        UpdateDelegate?.Invoke();

        try
        {
            IsRunning = await base.InitializeAsync();
            return IsRunning;
        }
        finally
        {
            IsInitializing = false;
            UpdateDelegate?.Invoke();
        }
    }

    #endregion

    protected override async Task ConfigureHubConnectionAsync()
    {
        await base.ConfigureHubConnectionAsync();

        HubConnection?.On(SignalREvents.DeviceApp.RequestDeviceRegistration, async () =>
        {
            var deviceInfo = new DeviceDto()
            {
                Id = DeviceId,
                DeviceName = GetDeviceName(),
                Fingerprint = GetFingerprint()
            };
            await HubConnection.InvokeAsync(SignalREvents.DeviceApp.RegisterDevice, deviceInfo);
        });
    }

    private static string GetDeviceName()
    {
        return $"{Build.Manufacturer ?? Build.Unknown} {Build.Model ?? Build.Unknown} ({Build.Device ?? Build.Unknown})";
    }

    private static string GetFingerprint()
    {
        var rawData = new List<string>
        {
            Build.Manufacturer ?? Build.Unknown,
            Build.Device ?? Build.Unknown,
            Build.Model ?? Build.Unknown,
            Build.Hardware ?? Build.Unknown,
            Build.Id ?? Build.Unknown,
            Build.Fingerprint ?? Build.Unknown,
            Build.Time.ToString()
        };

        var combined = string.Join("|", rawData);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        return Convert.ToBase64String(hash);
    }
}