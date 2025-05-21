using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.Shared.Constants;
using PaymentGateway.Shared.Services;
using PaymentGateway.Shared.Types;
using PaymentGateway.Shared.DTOs.Device;
using System.ComponentModel;
using Android.OS;
using LiteDB;
using PaymentGateway.PhoneApp.Types;

namespace PaymentGateway.PhoneApp.Services;

public class DeviceService : BaseSignalRService
{
    private readonly ILogger<DeviceService> _logger;
    private readonly LiteContext _context;
    
    public Guid DeviceId { get; }

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
    
    public string GetDeviceData()
    {
        return $"{Build.Manufacturer} {Build.Model} ({Build.VERSION.Release})";
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
    
    public void OnConnectionStateChanged(object? sender, bool e)
    {
        UpdateDelegate?.Invoke();
        _logger.LogDebug("Состояние сервиса изменилось на {State}", e);
    }
    
    protected override async Task ConfigureHubConnectionAsync()
    {
        await base.ConfigureHubConnectionAsync();

        HubConnection?.On(SignalREvents.DeviceApp.RequestDeviceRegistration, async () =>
        {
            var deviceInfo = new DeviceDto()
            {
                Id = DeviceId,
                DeviceData = GetDeviceData()
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