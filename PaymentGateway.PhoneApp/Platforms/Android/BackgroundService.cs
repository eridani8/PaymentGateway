using _Microsoft.Android.Resource.Designer;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.PhoneApp.Services;

namespace PaymentGateway.PhoneApp;

[Service(ForegroundServiceType = ForegroundService.TypeDataSync)]
public class BackgroundService : Service
{
    private ILogger<BackgroundService> _logger = null!;
    private DeviceService _deviceService = null!;
    private IBackgroundServiceManager _backgroundServiceManager = null!;
    private NotificationManager _notificationManager = null!;
    private ActionReceiver? _actionReceiver;
    private PowerManager.WakeLock? _wakeLock;

    public override IBinder? OnBind(Intent? intent)
    {
        return null;
    }

    public override void OnCreate()
    {
        base.OnCreate();
        _notificationManager = (GetSystemService(NotificationService) as NotificationManager)!;

        CreateNotificationChannel();

        var services = (ApplicationContext as IPlatformApplication)!.Services;

        _logger = services.GetRequiredService<ILogger<BackgroundService>>();
        _deviceService = services.GetRequiredService<DeviceService>();
        _backgroundServiceManager = services.GetRequiredService<IBackgroundServiceManager>();

        _deviceService.UpdateDelegate = UpdateNotification;

        _actionReceiver = new ActionReceiver();
        var filter = new IntentFilter();
        filter.AddAction(AndroidConstants.ActionStop);
        filter.AddAction(AndroidConstants.ActionStart);

        RegisterReceiver(_actionReceiver, filter, ReceiverFlags.NotExported);
    }

    public override void OnDestroy()
    {
        try
        {
            if (_actionReceiver != null)
            {
                UnregisterReceiver(_actionReceiver);
                _actionReceiver = null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }

        ReleaseWakeLock();

        _backgroundServiceManager.SetRunningState(false);

        base.OnDestroy();
    }

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        var initialNotification = BuildNotification(GetStatusText());
        StartForeground(AndroidConstants.NotificationId, initialNotification, ForegroundService.TypeDataSync);

        if (string.IsNullOrEmpty(_deviceService.AccessToken)) return StartCommandResult.Sticky;

        if (intent?.Action == AndroidConstants.ActionStop)
        {
            _ = StopBackgroundProcess();
            return StartCommandResult.Sticky;
        }
        
        if (!_backgroundServiceManager.IsRunning || intent?.Action == AndroidConstants.ActionStart)
        {
            _ = StartBackgroundProcess();
        }

        return StartCommandResult.Sticky;
    }

    private async Task StartBackgroundProcess()
    {
        _backgroundServiceManager.SetRunningState(true);
        _deviceService.IsInitializing = true;
        UpdateNotification();

        AcquireWakeLock();
        
        try
        {
            _deviceService.ConnectionStateChanged += _deviceService.OnConnectionStateChanged;
            await _deviceService.InitializeAsync();
        }
        finally
        {
            _deviceService.IsInitializing = false;
            UpdateNotification();
        }
    }

    private async Task StopBackgroundProcess()
    {
        _backgroundServiceManager.SetRunningState(false);
        await _deviceService.Stop();

        ReleaseWakeLock();
        UpdateNotification();
    }

    private void UpdateNotification()
    {
        var notification = BuildNotification(GetStatusText());
        _notificationManager.Notify(AndroidConstants.NotificationId, notification);
    }

    private string GetStatusText()
    {
        if (_deviceService.IsInitializing)
        {
            return "Инициализация";
        }

        if (string.IsNullOrEmpty(_deviceService.AccessToken))
        {
            return "Нужно задать токен";
        }

        if (_deviceService.IsServiceUnavailable)
        {
            return "Сервис недоступен";
        }

        return _backgroundServiceManager is { IsRunning: true }
            ? "Фоновой процесс активен"
            : "Фоновой процесс остановлен";
    }

    private string GetButtonText()
    {
        if (_deviceService.IsInitializing || _deviceService.IsServiceUnavailable)
        {
            return string.Empty;
        }

        if (string.IsNullOrEmpty(_deviceService.AccessToken))
        {
            return "Авторизация";
        }

        return _backgroundServiceManager is { IsRunning: true }
            ? "Остановить"
            : "Запустить";
    }

    private void AcquireWakeLock()
    {
        var powerManager = (PowerManager?)GetSystemService(PowerService);
        _wakeLock = powerManager?.NewWakeLock(WakeLockFlags.Partial, AndroidConstants.WakeLockTag);
        _wakeLock?.Acquire();
    }

    private void ReleaseWakeLock()
    {
        _wakeLock?.Release();
        _wakeLock = null;
    }

    private void CreateNotificationChannel()
    {
        var channel = new NotificationChannel(
            AndroidConstants.ChannelId,
            AndroidConstants.ChannelName,
            NotificationImportance.High)
        {
            LockscreenVisibility = NotificationVisibility.Public,
            Description = AndroidConstants.ChannelDescription
        };

        channel.SetShowBadge(true);
        channel.Importance = NotificationImportance.High;
        channel.EnableLights(true);
        channel.EnableVibration(true);

        _notificationManager.CreateNotificationChannel(channel);
    }

    private Notification BuildNotification(string statusText)
    {
        var contentIntent = new Intent(this, typeof(MainActivity));
        contentIntent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);

        var pendingIntent = PendingIntent.GetActivity(
            this, 0, contentIntent,
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);

        var stopIntent = new Intent(AndroidConstants.ActionStop);
        stopIntent.SetPackage(PackageName);
        var stopPendingIntent = PendingIntent.GetBroadcast(
            this, 1, stopIntent,
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);

        var startIntent = new Intent(AndroidConstants.ActionStart);
        startIntent.SetPackage(PackageName);
        var startPendingIntent = PendingIntent.GetBroadcast(
            this, 2, startIntent,
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);

        var authIntent = new Intent(this, typeof(MainActivity));
        authIntent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
        var authPendingIntent = PendingIntent.GetActivity(
            this, 3, authIntent,
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);

        var compatBuilder = new NotificationCompat.Builder(this, AndroidConstants.ChannelId)
            .SetContentText(statusText)
            .SetSmallIcon(ResourceConstant.Mipmap.appicon)
            .SetOngoing(true)
            .SetVisibility(NotificationCompat.VisibilityPublic)
            .SetCategory(NotificationCompat.CategoryService)
            .SetShowWhen(true)
            .SetPriority(NotificationCompat.PriorityHigh)
            .SetDefaults(NotificationCompat.DefaultAll)
            .SetColor(unchecked((int)0xFF7D5BA6))
            .SetForegroundServiceBehavior(NotificationCompat.ForegroundServiceImmediate)
            .SetContentIntent(pendingIntent);

        var style = new NotificationCompat.BigTextStyle().BigText(statusText);
        compatBuilder.SetStyle(style);

        var buttonText = GetButtonText();
        if (!string.IsNullOrEmpty(buttonText))
        {
            if (string.IsNullOrEmpty(_deviceService.AccessToken))
            {
                compatBuilder.AddAction(0, buttonText, authPendingIntent);
            }
            else if (_backgroundServiceManager is { IsRunning: true })
            {
                compatBuilder.AddAction(0, buttonText, stopPendingIntent);
            }
            else
            {
                compatBuilder.AddAction(0, buttonText, startPendingIntent);
            }
        }

        return compatBuilder.Build();
    }
}