using System.Text.Json.Serialization;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Services;

namespace PaymentGateway.Api.Configuration;

public static class SignalRConfiguration
{
    public static void ConfigureSignalR(WebApplicationBuilder builder)
    {
        builder.Services.AddSignalR(options =>
        {
            options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
            options.KeepAliveInterval = TimeSpan.FromSeconds(10);
            options.HandshakeTimeout = TimeSpan.FromSeconds(15);
            options.MaximumReceiveMessageSize = 1024 * 1024;
            options.EnableDetailedErrors = true;
            options.StreamBufferCapacity = 10;
        })
        .AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
            options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
        });
        
        builder.Services.AddScoped<INotificationService, NotificationService>();
    }
} 