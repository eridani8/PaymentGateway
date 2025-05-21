using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Uri = Android.Net.Uri;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace PaymentGateway.PhoneApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private bool _isFromNotificationSettings;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            RequestNotificationPermission();
        }

        RequestSmsPermissions();
        RequestNotificationListenerPermission();
        StartBackgroundService();
        RequestBatteryOptimizationIgnore();
    }

    protected override void OnResume()
    {
        base.OnResume();

        if (!_isFromNotificationSettings) return;
        _isFromNotificationSettings = false;
            
        if (IsNotificationListenerEnabled())
        {
            RestartBackgroundService();
        }
    }

    private void RequestNotificationPermission()
    {
        if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.PostNotifications) != Permission.Granted)
        {
            ActivityCompat.RequestPermissions(
                this,
                [Android.Manifest.Permission.PostNotifications],
                AndroidConstants.NotificationPermissionCode);
        }
    }

    private void RequestSmsPermissions()
    {
        var smsPermissions = new[]
        {
            Android.Manifest.Permission.ReceiveSms,
            Android.Manifest.Permission.ReadSms
        };

        var allSmsPermissionsGranted = smsPermissions.All(p => ContextCompat.CheckSelfPermission(this, p) == Permission.Granted);

        if (!allSmsPermissionsGranted)
        {
            ActivityCompat.RequestPermissions(this, smsPermissions, AndroidConstants.SmsPermissionCode);
        }
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        switch (requestCode)
        {
            case AndroidConstants.NotificationPermissionCode:
            {
                if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                {
                    RestartBackgroundService();
                }

                break;
            }
            case AndroidConstants.SmsPermissionCode:
            {
                var allGranted = grantResults.All(t => t == Permission.Granted);

                if (allGranted)
                {
                    RestartBackgroundService();
                }

                break;
            }
        }

        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }

    private void RestartBackgroundService()
    {
        try
        {
            var stopIntent = new Intent(this, typeof(BackgroundService));
            StopService(stopIntent);

            StartBackgroundService();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при перезапуске сервиса: {ex.Message}");
        }
    }

    private void StartBackgroundService()
    {
        var intent = new Intent(this, typeof(BackgroundService));
        intent.SetAction(AndroidConstants.ActionStart);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            StartForegroundService(intent);
        }
        else
        {
            StartService(intent);
        }
    }

    private void RequestBatteryOptimizationIgnore()
    {
        var powerManager = (PowerManager?)GetSystemService(PowerService);
        var packageName = PackageName;

        if (powerManager is null || powerManager.IsIgnoringBatteryOptimizations(packageName)) return;
        try
        {
            var intent = new Intent(Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations);
            intent.SetData(Uri.Parse($"package:{packageName}"));
            StartActivity(intent);
        }
        catch
        {
            // ignore
        }
    }

    private bool IsNotificationListenerEnabled()
    {
        var componentName = new ComponentName(this, Java.Lang.Class.FromType(typeof(NotificationListenerService)));
        var enabledListeners = Android.Provider.Settings.Secure.GetString(
            ContentResolver, 
            "enabled_notification_listeners");
            
        return enabledListeners is not null && enabledListeners.Contains(componentName.FlattenToString());
    }

    private void RequestNotificationListenerPermission()
    {
        if (IsNotificationListenerEnabled())
        {
            return;
        }
        
        try
        {
            var intent = new Intent(AndroidConstants.NotificationListenerSettingsAction);
            _isFromNotificationSettings = true;
            StartActivityForResult(intent, AndroidConstants.NotificationListenerSettingsCode);
        }
        catch
        {
            _isFromNotificationSettings = false;
        }
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        if (requestCode == AndroidConstants.NotificationListenerSettingsCode)
        {
            if (IsNotificationListenerEnabled())
            {
                RestartBackgroundService();
            }
        }
        
        base.OnActivityResult(requestCode, resultCode, data);
    }
}