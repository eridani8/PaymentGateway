namespace PaymentGateway.PhoneApp.Interfaces;

public interface ISmsProcessor
{
    Task ProcessSms(string sender, string message);
} 