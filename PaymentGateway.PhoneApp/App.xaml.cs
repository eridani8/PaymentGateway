using PaymentGateway.PhoneApp.Services;

namespace PaymentGateway.PhoneApp;

public partial class App : Application
{
    private readonly BackgroundServiceManager _serviceManager;

    public App(BackgroundServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        _ = _serviceManager.StartAllServicesAsync();
        var shell = new AppShell();
        return new Window(shell);
    }
}