namespace PaymentGateway.PhoneApp.Interfaces;

public interface ISmsProcessor
{
    void ProcessSms(string sender, string message);
} 