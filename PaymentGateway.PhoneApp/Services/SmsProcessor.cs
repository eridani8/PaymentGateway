using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;

namespace PaymentGateway.PhoneApp.Services;

public class SmsProcessor(ILogger<SmsProcessor> logger) : ISmsProcessor
{
    public void ProcessSms(string sender, string message)
    {
        logger.LogInformation($"Получено SMS от: {sender}\n{message}");
    }
} 