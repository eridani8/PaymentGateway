namespace PaymentGateway.PhoneApp.Interfaces;

public interface INotificationProcessor
{
    void ProcessNotification(string packageName, string notificationText, Dictionary<string, string>? additionalData = null);
} 