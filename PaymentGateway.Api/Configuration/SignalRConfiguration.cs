using System.Security.Claims;
using System.Text.Json.Serialization;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using PaymentGateway.Core.Configs;
using PaymentGateway.Core.Entities;

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
        if (authConfig is null) return;
        
        var key = Encoding.ASCII.GetBytes(authConfig.SecretKey);
            
        builder.Services.AddAuthentication()
            .AddJwtBearer("Device", options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = authConfig.Issuer,
                    ValidateAudience = true,
                    ValidAudience = authConfig.Audience,
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
                    },
                    OnTokenValidated = async context =>
                    {
                        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<UserEntity>>();
                        var userId = context.Principal?.FindFirst("i")?.Value;
                        
                        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out _))
                        {
                            context.Fail("Неверный идентификатор пользователя");
                            return;
                        }

                        var user = await userManager.FindByIdAsync(userId);
                        if (user is null)
                        {
                            context.Fail("Пользователь не найден");
                            return;
                        }

                        if (!user.IsActive)
                        {
                            context.Fail("Пользователь деактивирован");
                            return;
                        }
                    }
                };
            });
    }
} 