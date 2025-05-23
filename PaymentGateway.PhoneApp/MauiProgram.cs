using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.PhoneApp.Pages;
using PaymentGateway.PhoneApp.Services;
using PaymentGateway.PhoneApp.Types;
using PaymentGateway.PhoneApp.ViewModels;
using PaymentGateway.Shared.Types;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace PaymentGateway.PhoneApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseBarcodeReader()
            .ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<CameraBarcodeReaderView, CameraBarcodeReaderViewHandler>();
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("JetBrainsMono-Regular.ttf", "JetBrainsMono");
                fonts.AddFont("JetBrainsMono-Bold.ttf", "JetBrainsMono-Bold");
                fonts.AddFont("JetBrainsMono-Italic.ttf", "JetBrainsMono-Italic");
            });

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("PaymentGateway.PhoneApp.appsettings.json");

        if (stream is null)
        {
            throw new ApplicationException("Необходим конфигурационный файл");
        }

        var config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        builder.Configuration.AddConfiguration(config);
        builder.Services.Configure<AppSettings>(config.GetSection(nameof(AppSettings)));
        builder.Services.Configure<ApiSettings>(config.GetSection(nameof(ApiSettings)));
        var settings = builder.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();

        var liteContext = new LiteContext(settings!);
        var sink = new InMemoryLogSink(liteContext);

        var levelSwitch = new LoggingLevelSwitch(LogEventLevel.Debug); // TODO logs level
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(levelSwitch)
            .Enrich.FromLogContext()
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .WriteTo.Sink(sink)
            .CreateLogger();

        builder.Services.AddHttpClient("API", (serviceProvider, client) =>
        {
            var apiSettings = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;

            client.BaseAddress = new Uri(apiSettings.BaseAddress);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });
        
        builder.Services.Configure<JsonSerializerOptions>(options =>
        {
            options.ReferenceHandler = ReferenceHandler.Preserve;
            options.PropertyNameCaseInsensitive = true;
        });

        builder.Services.AddSingleton(new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            PropertyNameCaseInsensitive = true
        });

        builder.Services.AddSingleton(liteContext);
        builder.Services.AddSingleton(sink);

        builder.Services.AddSingleton<IAlertService, AlertService>();

        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<LogsViewModel>();
        builder.Services.AddSingleton<AuthorizationViewModel>();

        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<LogsPage>();
        builder.Services.AddSingleton<AuthorizationPage>();
        builder.Services.AddSingleton<QrScannerPage>();

        builder.Services.AddSingleton<ISmsProcessor, SmsProcessor>();
        builder.Services.AddSingleton<INotificationProcessor, NotificationProcessor>();

        builder.Services.AddSingleton<DeviceService>();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(dispose: true);

        return builder.Build();
    }
}