using Android.App;
using Android.Content;
using Android.Service.Notification;
using Android.OS;
using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;

namespace PaymentGateway.PhoneApp;

[Service(Name = "com.eridani8.paymentgateway.NotificationListenerService", 
    Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE",
    Label = "PaymentGateway Notification Listener",
    Exported = false)]
[IntentFilter(["android.service.notification.NotificationListenerService"])]
public class NotificationListenerService : Android.Service.Notification.NotificationListenerService
{
    private readonly ILogger<NotificationListenerService>? _logger;
    private readonly IBackgroundServiceManager? _backgroundServiceManager;
    private readonly IDeviceService? _deviceService;
    private readonly INotificationProcessor? _notificationProcessor;
    
    private readonly HashSet<string> _ignoredPackages =
    [
        "android", "com.android.systemui", "com.android.settings",
        "com.google.android.packageinstaller", "com.google.android.gms"
    ];
    
    public NotificationListenerService()
    {
        var app = Android.App.Application.Context as IPlatformApplication;
        if (app?.Services == null) return;
        
        _logger = app.Services.GetRequiredService<ILogger<NotificationListenerService>>();
        _backgroundServiceManager = app.Services.GetRequiredService<IBackgroundServiceManager>();
        _deviceService = app.Services.GetRequiredService<IDeviceService>();
        _notificationProcessor = app.Services.GetRequiredService<INotificationProcessor>();
    }
    
    public override void OnNotificationPosted(StatusBarNotification? sbn)
    {
        if (sbn == null) return;
        
        try
        {
            if (_notificationProcessor == null) return;
            if (_backgroundServiceManager is not { IsRunning: true }) return;
            if (_deviceService is not { State: true }) return;
            
            var packageName = sbn.PackageName;
            var notification = sbn.Notification;
            
            if (notification == null || string.IsNullOrEmpty(packageName)) return;
            if (packageName == PackageName) return;
            if (_ignoredPackages.Contains(packageName)) return;
            
            var additionalData = new Dictionary<string, string>
            {
                ["PostTime"] = DateTimeOffset.FromUnixTimeMilliseconds(sbn.PostTime).ToString("yyyy-MM-dd HH:mm:ss"),
                ["NotificationId"] = sbn.Id.ToString(),
                ["IsOngoing"] = sbn.IsOngoing.ToString()
            };
            
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                additionalData["Category"] = notification.Category ?? "none";
            }
            
            var extras = notification.Extras;
            if (extras == null) return;
            
            var title = extras.GetString(Notification.ExtraTitle)?.Trim();
            var text = extras.GetString(Notification.ExtraText)?.Trim();
            var subText = extras.GetString(Notification.ExtraSubText)?.Trim();
            var infoText = extras.GetString(Notification.ExtraInfoText)?.Trim();
            var summary = extras.GetString(Notification.ExtraSummaryText)?.Trim();
            
            if (extras.ContainsKey(Notification.ExtraBigText))
            {
                var bigText = extras.GetCharSequence(Notification.ExtraBigText)?.Trim();
                if (!string.IsNullOrEmpty(bigText))
                {
                    additionalData["BigText"] = bigText;
                }
            }
            
            var textParts = new List<string>();
            if (!string.IsNullOrEmpty(title)) textParts.Add(title);
            if (!string.IsNullOrEmpty(text)) textParts.Add(text);
            if (!string.IsNullOrEmpty(subText)) textParts.Add(subText);
            if (!string.IsNullOrEmpty(summary)) textParts.Add(summary);
            if (!string.IsNullOrEmpty(infoText)) textParts.Add(infoText);
            
            var fullText = string.Join(" ", textParts);
                
            if (string.IsNullOrEmpty(fullText)) return;
            
            try
            {
                var packageManager = PackageManager;
                if (packageManager != null)
                {
                    var appInfo = packageManager.GetApplicationInfo(packageName, 0);
                    var appName = packageManager.GetApplicationLabel(appInfo);
                    additionalData["AppName"] = appName;
                }
            }
            catch
            {
                // ignore
            }
            
            _notificationProcessor.ProcessNotification(packageName, fullText, additionalData);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Ошибка при обработке уведомления");
        }
    }
    
    public override void OnListenerConnected()
    {
        base.OnListenerConnected();
        _logger?.LogDebug("NotificationListenerService подключен");
    }
    
    public override void OnListenerDisconnected()
    {
        base.OnListenerDisconnected();
        _logger?.LogInformation("NotificationListenerService отключен");
        
        RequestRebind(new ComponentName(PackageName, Java.Lang.Class.FromType(typeof(NotificationListenerService)).Name));
    }
} 