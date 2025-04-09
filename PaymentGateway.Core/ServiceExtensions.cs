using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Core;

public static class ServiceExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CryptographyConfig>(configuration.GetSection(nameof(CryptographyConfig)));
        services.Configure<AuthConfig>(configuration.GetSection(nameof(AuthConfig)));
    
        services.AddScoped<ICryptographyService, CryptographyService>();

        return services;
    }
}