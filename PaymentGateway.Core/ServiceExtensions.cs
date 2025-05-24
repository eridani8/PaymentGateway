using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Core.Configs;
using PaymentGateway.Core.Encryption;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared;
using PaymentGateway.Shared.Types;

namespace PaymentGateway.Core;

public static class ServiceExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CryptographyConfig>(configuration.GetSection(nameof(CryptographyConfig)));
        services.Configure<AuthConfig>(configuration.GetSection(nameof(AuthConfig)));
    
        services.AddSingleton<ICryptographyService, CryptographyService>();

        return services;
    }
}