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
            options.ClientTimeoutInterval = TimeSpan.FromMinutes(60);
            options.KeepAliveInterval = TimeSpan.FromMinutes(1);
            options.HandshakeTimeout = TimeSpan.FromMinutes(1);
            options.MaximumReceiveMessageSize = 1024 * 1024;
            options.EnableDetailedErrors = true;
            options.StreamBufferCapacity = 20;
        })
        .AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
            options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
        })
        .AddMessagePackProtocol();
        
        builder.Services.AddScoped<INotificationService, NotificationService>();
    }
} 