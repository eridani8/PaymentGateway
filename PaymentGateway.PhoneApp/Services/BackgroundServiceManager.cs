using PaymentGateway.PhoneApp.Interfaces;

namespace PaymentGateway.PhoneApp.Services;

public class BackgroundServiceManager : IBackgroundServiceManager
{
    public bool IsRunning { get; private set; }
    public void SetRunningState(bool isRunning)
    {
        IsRunning = isRunning;
    }
} 