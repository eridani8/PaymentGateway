using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Pages;
using PaymentGateway.PhoneApp.Resources.Fonts;
using PaymentGateway.PhoneApp.Services.Logs;
using PaymentGateway.PhoneApp.ViewModels;
using Serilog;
using Serilog.Core;

namespace PaymentGateway.PhoneApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var levelSwitch = new LoggingLevelSwitch();
        var sink = new InMemoryLogSink();
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(levelSwitch)
            .Enrich.FromLogContext()
            .WriteTo.Sink(sink)
            .CreateLogger();

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureMauiHandlers(_ => { })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
            });

        builder.Services.AddSingleton(sink);

        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();
        
        builder.Services.AddTransient<LogsViewModel>();
        builder.Services.AddTransient<LogsPage>();

#if DEBUG
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(dispose: true);
#endif

        return builder.Build();
    }
}