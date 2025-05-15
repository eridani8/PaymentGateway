using System.Globalization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteDB;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.PhoneApp.Services;
using PaymentGateway.PhoneApp.Types;

namespace PaymentGateway.PhoneApp.ViewModels;

public partial class DeviceIdViewModel : BaseViewModel
{
    [ObservableProperty] private string _deviceId;

    public DeviceIdViewModel(LiteContext context, IDeviceService deviceService)
    {
        if (context.KeyValues.FindOne(k => k.Key == "DeviceId") is not { } keyValue)
        {
            keyValue = new KeyValue()
            {
                Id = ObjectId.NewObjectId(),
                Key = "DeviceId",
                Value = Guid.CreateVersion7()
            };
            context.KeyValues.Insert(keyValue);
        }

        if (keyValue.Value is Guid guid)
        {
            _deviceId = guid.ToString();
            _ = deviceService.SendDeviceId(guid);
        }
        else
        {
            _deviceId = "NULL";
        }
    }
    
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