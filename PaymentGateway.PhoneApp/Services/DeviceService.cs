using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentGateway.Shared.Constants;
using PaymentGateway.Shared.Services;
using PaymentGateway.Shared.Types;
using PaymentGateway.Shared.DTOs.Device;
using Android.OS;
using LiteDB;
using PaymentGateway.PhoneApp.Types;

namespace PaymentGateway.PhoneApp.Services;

public class DeviceService : BaseSignalRService
{
    private readonly ILogger<DeviceService> _logger;
    private readonly LiteContext _context;

    private Guid DeviceId { get; }

    public Action? UpdateDelegate;

    public DeviceService(IOptions<ApiSettings> settings,
        ILogger<DeviceService> logger,
        LiteContext context) : base(settings, logger)
    {
        _logger = logger;
        _context = context;
        AccessToken = context.GetToken();
        DeviceId = context.GetDeviceId();
    }
    
    public void SaveToken()
    {
        _context.KeyValues.Insert(new KeyValue()
        {
            Id = ObjectId.NewObjectId(),
            Key = LiteContext.tokenKey,
            Value = AccessToken
        });
    }

    public void RemoveToken()
    {
        if (_context.KeyValues.FindOne(e => e.Key == LiteContext.tokenKey) is { } keyValue)
        {
            _context.KeyValues.Delete(keyValue.Id);
        }
    }

    private static string GetDeviceName()
    {
        return $"{Build.Manufacturer ?? Build.Unknown} {Build.Model ?? Build.Unknown} {Build.Hardware ?? Build.Unknown}";
    }

    private static string GetHw()
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
        return Convert.ToHexString(hash).ToUpperInvariant();
    }

    public async Task Stop()
    {
        try
        {
            IsInitializing = false;
            UpdateDelegate?.Invoke();
            await StopAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Ошибка отключения");
        }
    }
    
    protected override async Task ConfigureHubConnectionAsync()
    {
        await base.ConfigureHubConnectionAsync();

        HubConnection?.On(SignalREvents.DeviceApp.RequestDeviceRegistration, async () =>
        {
            var deviceInfo = new DeviceDto()
            {
                Id = DeviceId,
                DeviceName = GetDeviceName(),
                Hw = GetHw()
            };
            await HubConnection.InvokeAsync(SignalREvents.DeviceApp.RegisterDevice, deviceInfo);
        });
    }

    public override async Task<bool> InitializeAsync()
    {
        IsInitializing = true;
        UpdateDelegate?.Invoke();
        
        try
        {
            return await base.InitializeAsync();
        }
        finally
        {
            IsInitializing = false;
            UpdateDelegate?.Invoke();
        }
    }
}