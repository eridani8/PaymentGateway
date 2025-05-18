using System.Collections.Concurrent;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Mappings;
using PaymentGateway.Application.Services;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared.Validations;

namespace PaymentGateway.Application;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(RequisiteProfile));
        services.AddAutoMapper(typeof(PaymentProfile));
        services.AddAutoMapper(typeof(TransactionProfile));
        services.AddAutoMapper(typeof(UserProfile));
        services.AddAutoMapper(typeof(ChatMessageProfile));
        services.AddAutoMapper(typeof(DeviceProfile));

        services.AddValidatorsFromAssembly(typeof(BaseValidator<>).Assembly);
        
        services.AddScoped<IRequisiteService, RequisiteService>();
        
        services.AddScoped<IPaymentConfirmationService, PaymentConfirmationService>();
        services.AddScoped<IPaymentService, PaymentService>();

        services.AddScoped<ITransactionService, TransactionService>();

        services.AddSingleton<ITokenService, TokenService>();
        
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IChatMessageService, ChatMessageService>();
        
        services.AddScoped<IGatewayHandler, GatewayHandler>();

        services.AddHostedService<GatewayHost>();

        return services;
    }
}