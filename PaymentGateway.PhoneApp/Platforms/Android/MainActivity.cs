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
    private const string ActionStart = "com.eridani8.paymentgateway.START_SERVICE";
    private const int NotificationPermissionCode = 100;
    private const int SmsPermissionCode = 200;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            RequestNotificationPermission();
        }

        RequestSmsPermissions();
        StartBackgroundService();
        RequestBatteryOptimizationIgnore();
    }

    private void RequestNotificationPermission()
    {
        if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.PostNotifications) !=
            Permission.Granted)
        {
            ActivityCompat.RequestPermissions(
                this,
                [Android.Manifest.Permission.PostNotifications],
                NotificationPermissionCode);
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
            ActivityCompat.RequestPermissions(this, smsPermissions, SmsPermissionCode);
        }
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        switch (requestCode)
        {
            case NotificationPermissionCode:
            {
                if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                {
                    RestartBackgroundService();
                }

                break;
            }
            case SmsPermissionCode:
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
        intent.SetAction(ActionStart);

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

        if (powerManager == null || powerManager.IsIgnoringBatteryOptimizations(packageName)) return;
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
}