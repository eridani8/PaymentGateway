using PaymentGateway.PhoneApp.Pages;

namespace PaymentGateway.PhoneApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();
    }

    private static void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(LogsPage), typeof(LogsPage));
        Routing.RegisterRoute(nameof(ServiceUnavailablePage), typeof(ServiceUnavailablePage));
    }
}