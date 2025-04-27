using Android.Content;

namespace PaymentGateway.PhoneApp;

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