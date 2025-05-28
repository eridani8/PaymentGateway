using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Infrastructure.Interfaces;
using PaymentGateway.Infrastructure.Repositories;
using PaymentGateway.Infrastructure.Repositories.Cached;

namespace PaymentGateway.Infrastructure;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContextPool<AppDbContext>(o =>
        {
            o.UseNpgsql(connectionString);
        });

        services.AddIdentity<UserEntity, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
    
        services.AddMemoryCache();

        services.AddScoped<PaymentRepository>();
        services.AddScoped<IPaymentRepository, CachedPaymentRepository>();
        
        services.AddScoped<RequisiteRepository>();
        services.AddScoped<IRequisiteRepository, CachedRequisiteRepository>();

        services.AddScoped<TransactionRepository>();
        services.AddScoped<ITransactionRepository, CachedTransactionRepository>();
        
        services.AddScoped<ChatRepository>();
        services.AddScoped<IChatRepository, CachedChatRepository>();

        services.AddScoped<DeviceRepository>();
        services.AddScoped<IDeviceRepository, CachedDeviceRepository>();

        services.AddScoped<ISettingsRepository, SettingsRepository>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }
}