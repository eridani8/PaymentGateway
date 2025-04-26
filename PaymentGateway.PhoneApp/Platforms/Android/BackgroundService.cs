using _Microsoft.Android.Resource.Designer;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;

namespace PaymentGateway.PhoneApp;

[Service(ForegroundServiceType = ForegroundService.TypeDataSync)]
public class BackgroundService : Service
{
    private const int NotificationId = 99999;
    private const string ChannelId = "PaymentGatewayChannel";
    private const string ChannelName = "Фоновая работа";
    private const string ActionStop = "com.eridani8.paymentgateway.STOP_SERVICE";
    private const string ActionStart = "com.eridani8.paymentgateway.START_SERVICE";

    private IAvailabilityChecker? _availabilityChecker;
    private ILogger<BackgroundService>? _logger;
    private PowerManager.WakeLock? _wakeLock;
    private bool _isRunning;
    private bool _previousServiceState;
    private NotificationManager? _notificationManager;
    private Timer? _backgroundTimer;
    private ActionReceiver? _actionReceiver;

    public override IBinder? OnBind(Intent? intent)
    {
        return null;
    }

    public override void OnCreate()
    {
        base.OnCreate();
        _notificationManager = GetSystemService(NotificationService) as NotificationManager;
        
        CreateNotificationChannel();
        
        var services = (ApplicationContext as IPlatformApplication)?.Services;
        if (services != null)
        {
            _logger = services.GetRequiredService<ILogger<BackgroundService>>();
            _availabilityChecker = services.GetRequiredService<IAvailabilityChecker>();
            if (_availabilityChecker != null)
            {
                _previousServiceState = _availabilityChecker.State;
            }
        }
        
        _actionReceiver = new ActionReceiver();
        var filter = new IntentFilter();
        filter.AddAction(ActionStop);
        filter.AddAction(ActionStart);
        
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
            _logger?.LogError(e, e.Message);
        }
        
        StopBackgroundTimer();
        ReleaseWakeLock();
        base.OnDestroy();
    }

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        if (intent?.Action == ActionStop)
        {
            StopBackgroundProcess();
            return StartCommandResult.Sticky;
        }
        
        if (!_isRunning || intent?.Action == ActionStart)
        {
            StartBackgroundProcess();
        }
        
        return StartCommandResult.Sticky;
    }

    private void StartBackgroundProcess()
    {
        _isRunning = true;
        var notification = BuildNotification(GetStatusText());
        
        StartForeground(NotificationId, notification, ForegroundService.TypeDataSync);

        AcquireWakeLock();
        
        StartBackgroundTimer();
    }
    
    private void StopBackgroundProcess()
    {
        _isRunning = false;
        StopBackgroundTimer();
        
        var notification = BuildNotification(GetStatusText());
        _notificationManager?.Notify(NotificationId, notification);
        
        ReleaseWakeLock();
    }
    
    private void StartBackgroundTimer()
    {
        StopBackgroundTimer();
        
        _backgroundTimer = new Timer(async void (_) =>
        {
            try
            {
                await DoBackgroundWork();
            }
            catch (Exception e)
            {
                _logger?.LogError(e, e.Message);
            }
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
    }
    
    private void StopBackgroundTimer()
    {
        if (_backgroundTimer == null) return;
        _backgroundTimer.Dispose();
        _backgroundTimer = null;
    }
    
    private async Task DoBackgroundWork()
    {
        try
        {
            var previousState = _availabilityChecker?.State ?? false;
            
            await _availabilityChecker!.BackgroundCheckAsync();
            
            var currentState = _availabilityChecker.State;
            if (_isRunning && (previousState != currentState || _previousServiceState != currentState))
            {
                _previousServiceState = currentState;
                _logger?.LogInformation("Состояние сервиса изменилось на {State}", currentState);
                UpdateNotification();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Ошибка при выполнении фоновой задачи");
        }
    }

    private void UpdateNotification()
    {
        if (_notificationManager == null || _availabilityChecker == null) return;
        var statusText = GetStatusText();
        var notification = BuildNotification(statusText);
        _notificationManager.Notify(NotificationId, notification);
    }

    private string GetStatusText()
    {
        var processStatus = _isRunning ? "Фоновой процесс активен" : "Фоновой процесс остановлен";
        
        if (_availabilityChecker == null)
            return processStatus;
        
        var serviceStatus = _availabilityChecker.State ? "Сервис доступен" : "Сервис недоступен";
        
        return $"{processStatus}\n{serviceStatus}";
    }

    private void AcquireWakeLock()
    {
        if (_wakeLock != null) return;
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
            ChannelId, 
            ChannelName, 
            NotificationImportance.High)
        {
            LockscreenVisibility = NotificationVisibility.Public,
            Description = "Управление фоновыми процессами приложения и мониторинг доступности сервиса"
        };

        channel.SetShowBadge(true);
        channel.Importance = NotificationImportance.High;
        channel.EnableLights(true);
        channel.EnableVibration(true);
        
        _notificationManager?.CreateNotificationChannel(channel);
    }

    private Notification BuildNotification(string statusText)
    {
        var contentIntent = new Intent(this, typeof(MainActivity));
        contentIntent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
        
        var pendingIntent = PendingIntent.GetActivity(
            this, 0, contentIntent, 
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);
        
        var stopIntent = new Intent(ActionStop);
        stopIntent.SetPackage(PackageName);
        var stopPendingIntent = PendingIntent.GetBroadcast(
            this, 1, stopIntent,
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);
            
        var startIntent = new Intent(ActionStart);
        startIntent.SetPackage(PackageName);
        var startPendingIntent = PendingIntent.GetBroadcast(
            this, 2, startIntent,
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);
        
        var compatBuilder = new NotificationCompat.Builder(this, ChannelId)
            .SetContentTitle("PaymentGateway")
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
        
        if (_isRunning)
        {
            compatBuilder.AddAction(0, "Остановить", stopPendingIntent);
        }
        else
        {
            compatBuilder.AddAction(0, "Запустить", startPendingIntent);
        }
        
        return compatBuilder.Build();
    }
    
    [BroadcastReceiver(Exported = false)]
    public class ActionReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context? context, Intent? intent)
        {
            if (context == null || intent == null) return;
            
            var serviceIntent = new Intent(context, typeof(BackgroundService));
            serviceIntent.SetAction(intent.Action);
            
            context.StartForegroundService(serviceIntent);
        }
    }
} 