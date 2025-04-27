using Android.App;
using Android.Content;
using Android.Provider;
using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;

namespace PaymentGateway.PhoneApp;

[BroadcastReceiver(Enabled = true, Exported = true, Name = "com.eridani8.paymentgateway.SmsReceiver")]
[IntentFilter(["android.provider.Telephony.SMS_RECEIVED"])]
public class SmsReceiver : BroadcastReceiver
{
    private readonly ILogger<SmsReceiver>? _logger;
    private readonly ISmsProcessor? _smsProcessor;
    private readonly IBackgroundServiceManager? _backgroundServiceManager;
    private IAvailabilityChecker? _availabilityChecker;

    public SmsReceiver()
    {
        var app = Android.App.Application.Context as IPlatformApplication;
        if (app?.Services == null) return;
        
        _logger = app.Services.GetRequiredService<ILogger<SmsReceiver>>();
        _smsProcessor = app.Services.GetRequiredService<ISmsProcessor>();
        _availabilityChecker = app.Services.GetRequiredService<IAvailabilityChecker>();
        _backgroundServiceManager = app.Services.GetRequiredService<IBackgroundServiceManager>();
    }

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null || intent is not { Action: "android.provider.Telephony.SMS_RECEIVED" }) return;

        try
        {
            if (_smsProcessor == null) return;
            if (_backgroundServiceManager is not { IsRunning: true }) return;
            if (_availabilityChecker is not { State: true }) return;

            var messages = Telephony.Sms.Intents.GetMessagesFromIntent(intent);
            if (messages == null || messages.Length == 0) return;

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
                    _logger?.LogError(e, "SmsReceiver: Ошибка при обработке SMS");
                }
            }
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "SmsReceiver: Ошибка при получении SMS");
        }
    }
} 