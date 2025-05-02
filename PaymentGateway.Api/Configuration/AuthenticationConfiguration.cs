using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Api.Configuration;

public static class AuthenticationConfiguration
{
    public static void ConfigureAuthentication(WebApplicationBuilder builder, Core.AuthConfig authConfig)
    {
        var secretKey = authConfig.SecretKey;
        var key = Encoding.ASCII.GetBytes(secretKey);
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
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
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<UserEntity>>();
                        var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        
                        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out _))
                        {
                            context.Fail("Неверный идентификатор пользователя");
                            return;
                        }

                        var user = await userManager.FindByIdAsync(userId);
                        if (user == null)
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