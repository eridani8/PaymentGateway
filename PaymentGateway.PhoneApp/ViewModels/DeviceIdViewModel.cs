using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteDB;
using PaymentGateway.PhoneApp.Services;
using PaymentGateway.PhoneApp.Types;

namespace PaymentGateway.PhoneApp.ViewModels;

public partial class DeviceIdViewModel : BaseViewModel
{
    [ObservableProperty] private string _deviceId;

    public DeviceIdViewModel(LiteContext context)
    {
        if (context.KeyValues.FindOne(k => k.Key == "DeviceId") is not { } keyValue)
        {
            keyValue = new KeyValue()
            {
                Id = ObjectId.NewObjectId(),
                Key = "DeviceId",
                Value = Guid.CreateVersion7().ToString()
            };
            context.KeyValues.Insert(keyValue);
        }

        _deviceId = keyValue.Value!.ToString()!;
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