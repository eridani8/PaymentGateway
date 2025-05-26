using Asp.Versioning.ApiExplorer;
using Carter;
using PaymentGateway.Api;
using PaymentGateway.Api.Configuration;
using PaymentGateway.Application.Extensions;
using PaymentGateway.Application.Hubs;
using PaymentGateway.Core;
using PaymentGateway.Core.Configs;
using PaymentGateway.Infrastructure;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);

    var connectionString = builder.Configuration.GetConnectionString("Default");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new ApplicationException("Нужно указать строку подключения базы данных");
    }

    var authConfig = builder.Configuration.GetSection(nameof(AuthConfig)).Get<AuthConfig>();
    if (authConfig is null)
    {
        throw new ApplicationException("Нужно указать настройки аутентификации");
    }
    
    var otlpConfig = builder.Configuration.GetSection(nameof(OpenTelemetryConfig)).Get<OpenTelemetryConfig>();
    if (otlpConfig is null)
    {
        throw new ApplicationException("Нужно указать настройки OpenTelemetry");
    }
    
    OpenTelemetryConfiguration.Configure(builder, otlpConfig);
    
    Log.Logger = LoggingConfiguration.ConfigureLogger(connectionString, otlpConfig);
    builder.Host.UseSerilog(Log.Logger);

    builder.Services.Configure<GatewayConfig>(builder.Configuration.GetSection(nameof(GatewayConfig)));

    SwaggerConfiguration.ConfigureSwagger(builder);
    builder.Services.AddCarter();
    ApiVersioningConfiguration.ConfigureApiVersioning(builder);

    builder.Services.AddCore(builder.Configuration);
    builder.Services.AddInfrastructure(connectionString);
    builder.Services.AddApplication();

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    JsonConfiguration.ConfigureJson(builder);
    SignalRConfiguration.ConfigureSignalR(builder);
    AuthenticationConfiguration.ConfigureAuthentication(builder, authConfig);

    CorsConfiguration.ConfigureCors(builder);

    var app = builder.Build();

    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });

    app.UseCors("AllowAll");
    
    // app.UseHttpsRedirection(); // TODO https connection
    
    app.UseWebSockets(new WebSocketOptions
    {
        KeepAliveInterval = TimeSpan.FromSeconds(10)
    });

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseExceptionHandler();

    app.MapHub<NotificationHub>("/notificationHub");
    app.MapHub<DeviceHub>("/deviceHub");

    app.MapCarter();
    
    await AppConfiguration.InitializeApplication(app);

    await app.RunAsync();
}
catch (Exception e)
{
    Log.Fatal(e, "Сервис не смог запуститься");
}
finally
{
    await Log.CloseAndFlushAsync();
}