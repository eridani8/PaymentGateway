using System.Text.Json.Serialization;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PaymentGateway.Core.Configs;

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
        
        var authConfig = builder.Configuration.GetSection(nameof(AuthConfig)).Get<AuthConfig>();
        if (authConfig is not null)
        {
            var key = Encoding.ASCII.GetBytes(authConfig.SecretKey);
            
            builder.Services.AddAuthentication()
                .AddJwtBearer("Device", options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/deviceHub"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
        }
    }
} 