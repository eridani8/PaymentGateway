namespace PaymentGateway.PhoneApp.Interfaces;

public interface IBackgroundServiceManager
{
    bool IsRunning { get; }
    void SetRunningState(bool isRunning);
} 