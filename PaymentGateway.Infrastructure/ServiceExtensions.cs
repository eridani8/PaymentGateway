using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Core.Interfaces.Repositories;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Infrastructure.Interfaces;
using PaymentGateway.Infrastructure.Repositories;

namespace PaymentGateway.Infrastructure;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(o =>
        {
            o.UseNpgsql(connectionString);
        });

        services.AddIdentity<UserEntity, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
    
        services.AddMemoryCache();
        services.AddSingleton<ICache, InMemoryCache>();
        
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IRequisiteRepository, RequisiteRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }
}