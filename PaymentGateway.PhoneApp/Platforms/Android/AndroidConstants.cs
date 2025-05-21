namespace PaymentGateway.PhoneApp;

public static class AndroidConstants
{
    public const string ActionStop = "com.eridani8.paymentgateway.STOP_SERVICE";
    public const string ActionStart = "com.eridani8.paymentgateway.START_SERVICE";
    public const int NotificationId = 9999;
    public const string ChannelId = "PaymentGatewayChannel";
    public const string ChannelName = "PaymentGatewayForegroundService";
    public const string ChannelDescription = "Фоновый процесс отслеживания уведомлений и СМС";
    public const string WakeLockTag = "PaymentGateway::BackgroundServiceLock";
    
    public const int NotificationPermissionCode = 100;
    public const int SmsPermissionCode = 200;
    public const int NotificationListenerSettingsCode = 300;
    public const string NotificationListenerSettingsAction = "android.settings.ACTION_NOTIFICATION_LISTENER_SETTINGS";

    public const string NotificationListenerServiceName = "com.eridani8.paymentgateway.NotificationListenerService";
    public const string NotificationListenerServicePermission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE";
    public const string NotificationListenerServiceLabel = "PaymentGateway Notification Listener";
    public const string NotificationListenerServiceIntentFilter = "android.service.notification.NotificationListenerService";
    
    public const string SmsReceiverServiceName = "com.eridani8.paymentgateway.SmsReceiver";
    public const string SmsReceiverServiceIntentFilter = "android.provider.Telephony.SMS_RECEIVED";
} 