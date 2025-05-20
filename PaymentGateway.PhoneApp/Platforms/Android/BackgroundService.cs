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
    private const int notificationId = 9999;
    private const string channelId = "PaymentGatewayChannel";
    private const string channelName = "PaymentGatewayForegroundService";
    private const string actionStop = "com.eridani8.paymentgateway.STOP_SERVICE";
    private const string actionStart = "com.eridani8.paymentgateway.START_SERVICE";

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
        filter.AddAction(actionStop);
        filter.AddAction(actionStart);
        
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
        var initialNotification = BuildNotification("Инициализация сервиса...");
        StartForeground(notificationId, initialNotification, ForegroundService.TypeDataSync);
        
        if (string.IsNullOrEmpty(_deviceService.AccessToken))
        {
            var notification = BuildNotification("Нужно задать токен");
            _notificationManager.Notify(notificationId, notification);
            _logger.LogWarning("Задайте токен");
            return StartCommandResult.Sticky;
        }
        
        
        if (intent?.Action == actionStop)
        {
            _ = StopBackgroundProcess();
            return StartCommandResult.Sticky;
        }
        
        if (!_backgroundServiceManager.IsRunning || intent?.Action == actionStart)
        {
            _ = StartBackgroundProcess();
        }

        return StartCommandResult.Sticky;
    }

    private async Task StartBackgroundProcess()
    {
        var notification = BuildNotification(GetStatusText());
        _notificationManager.Notify(notificationId, notification);

        _backgroundServiceManager.SetRunningState(true);

        AcquireWakeLock();
        try
        {
            _deviceService.ConnectionStateChanged += _deviceService.OnConnectionStateChanged;
            await _deviceService.InitializeAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Ошибка подключения к сервису");
        }
    }

    private async Task StopBackgroundProcess()
    {
        _backgroundServiceManager.SetRunningState(false);
        var notification = BuildNotification(GetStatusText());
        _notificationManager.Notify(notificationId, notification);

        await _deviceService.Stop();
        
        ReleaseWakeLock();
    }

    private void UpdateNotification()
    {
        var statusText = GetStatusText();
        var notification = BuildNotification(statusText);
        _notificationManager.Notify(notificationId, notification);
    }

    private string GetStatusText()
    {
        var processStatus = _backgroundServiceManager is { IsRunning: true } ? "Фоновой процесс активен" : "Фоновой процесс остановлен";
        var serviceStatus = _deviceService.IsConnected ? "Сервис доступен" : "Сервис недоступен";
        return $"{processStatus}\n{serviceStatus}";
    }

    private void AcquireWakeLock()
    {
        var powerManager = (PowerManager?)GetSystemService(PowerService);
        _wakeLock = powerManager?.NewWakeLock(WakeLockFlags.Partial, "PaymentGateway::BackgroundServiceLock");
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
            channelId, 
            channelName, 
            NotificationImportance.High)
        {
            LockscreenVisibility = NotificationVisibility.Public,
            Description = "Управление фоновыми процессами приложения и мониторинг доступности сервиса"
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
        
        var stopIntent = new Intent(actionStop);
        stopIntent.SetPackage(PackageName);
        var stopPendingIntent = PendingIntent.GetBroadcast(
            this, 1, stopIntent,
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);
            
        var startIntent = new Intent(actionStart);
        startIntent.SetPackage(PackageName);
        var startPendingIntent = PendingIntent.GetBroadcast(
            this, 2, startIntent,
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);
        
        var compatBuilder = new NotificationCompat.Builder(this, channelId)
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
        
        if (_backgroundServiceManager is { IsRunning: true })
        {
            compatBuilder.AddAction(0, "Остановить", stopPendingIntent);
        }
        else
        {
            compatBuilder.AddAction(0, "Запустить", startPendingIntent);
        }
        
        return compatBuilder.Build();
    }
} 