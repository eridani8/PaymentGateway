using Android.App;
using Android.Content;
using Android.Provider;
using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.PhoneApp.Services;
using PaymentGateway.Shared.Services;

namespace PaymentGateway.PhoneApp;

[BroadcastReceiver(Enabled = true, Exported = true, Name = AndroidConstants.SmsReceiverServiceName)]
[IntentFilter([AndroidConstants.SmsReceiverServiceIntentFilter])]
public class SmsReceiver : BroadcastReceiver
{
    private readonly ILogger<SmsReceiver> _logger = null!;
    private readonly ISmsProcessor _smsProcessor = null!;
    private readonly IBackgroundServiceManager _backgroundServiceManager = null!;
    private readonly DeviceService _deviceService = null!;

    public SmsReceiver()
    {
        var app = Android.App.Application.Context as IPlatformApplication;
        if (app?.Services is null) return;
        
        _logger = app.Services.GetRequiredService<ILogger<SmsReceiver>>();
        _smsProcessor = app.Services.GetRequiredService<ISmsProcessor>();
        _deviceService = app.Services.GetRequiredService<DeviceService>();
        _backgroundServiceManager = app.Services.GetRequiredService<IBackgroundServiceManager>();
    }

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null || intent is not { Action: AndroidConstants.SmsReceiverServiceIntentFilter }) return;

        try
        {
            if (_backgroundServiceManager is not { IsRunning: true }) return;
            if (_deviceService is not { IsConnected: true }) return;

            var messages = Telephony.Sms.Intents.GetMessagesFromIntent(intent);
            if (messages is null || messages.Length == 0) return;

            foreach (var message in messages)
            {
                try
                {
                    var senderPhone = message.DisplayOriginatingAddress ?? string.Empty;
                    var messageBody = message.DisplayMessageBody ?? string.Empty;

                    _smsProcessor.ProcessSms(senderPhone, messageBody);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Ошибка при обработке SMS");
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Ошибка при обработке SMS");
        }
    }
} 