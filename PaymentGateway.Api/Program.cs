using Carter;
using PaymentGateway.Api;
using PaymentGateway.Api.Configuration;
using PaymentGateway.Application;
using PaymentGateway.Application.Hubs;
using PaymentGateway.Core;
using PaymentGateway.Infrastructure;
using Serilog;
using Asp.Versioning.ApiExplorer;

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

    Log.Logger = LoggingConfiguration.ConfigureLogger(connectionString);
    builder.Host.UseSerilog(Log.Logger);

    builder.Services.Configure<GatewaySettings>(builder.Configuration.GetSection(nameof(GatewaySettings)));

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

    app.UseCors("AllowAll"); // TODO

    // app.UseHttpsRedirection(); // TODO
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseExceptionHandler();

    app.MapHub<NotificationHub>("/notificationHub");

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