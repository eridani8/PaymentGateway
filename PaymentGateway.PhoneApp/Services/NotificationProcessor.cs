using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;

namespace PaymentGateway.PhoneApp.Services;

public class NotificationProcessor(ILogger<NotificationProcessor> logger) : INotificationProcessor
{
    public void ProcessNotification(string packageName, string notificationText, Dictionary<string, string>? additionalData = null)
    {
        logger.LogInformation($"Получено уведомление от: {packageName}\n{notificationText}");
        
        if (additionalData is { Count: > 0 })
        {
            var additionalInfo = string.Join(", ", additionalData.Select(kv => $"{kv.Key}: {kv.Value}"));
            logger.LogDebug($"Дополнительная информация: {additionalInfo}");
        }
    }
} 