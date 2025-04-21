using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Mappings;
using PaymentGateway.Application.Services;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.Transaction;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Interfaces;
using PaymentGateway.Shared.Validations.Validators.Payment;
using PaymentGateway.Shared.Validations.Validators.Requisite;
using PaymentGateway.Shared.Validations.Validators.Transaction;
using PaymentGateway.Shared.Validations.Validators.User;

namespace PaymentGateway.Application;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(RequisiteProfile));
        services.AddAutoMapper(typeof(PaymentProfile));
        services.AddAutoMapper(typeof(TransactionProfile));
        services.AddAutoMapper(typeof(UserProfile));
        
        services.AddScoped<IValidator<RequisiteCreateDto>, RequisiteCreateDtoValidator>();
        services.AddScoped<IValidator<RequisiteUpdateDto>, RequisiteUpdateDtoValidator>();
        services.AddScoped<IRequisiteService, RequisiteService>();
        
        services.AddScoped<IValidator<PaymentCreateDto>, PaymentCreateDtoValidator>();
        services.AddScoped<IValidator<PaymentManualConfirmDto>, PaymentManualConfirmDtoValidator>();
        services.AddScoped<IValidator<PaymentCancelDto>, PaymentCancelDtoValidator>();
        services.AddScoped<IPaymentConfirmationService, PaymentConfirmationService>();
        services.AddScoped<IPaymentService, PaymentService>();

        services.AddScoped<IValidator<TransactionCreateDto>, TransactionCreateDtoValidator>();
        services.AddScoped<ITransactionService, TransactionService>();

        services.AddScoped<IValidator<LoginDto>, LoginModelValidator>();
        services.AddScoped<IValidator<ChangePasswordDto>, ChangePasswordValidator>();
        services.AddScoped<IValidator<CreateUserDto>, CreateUserValidator>();
        services.AddScoped<IValidator<UpdateUserDto>, UpdateUserValidator>();
        services.AddScoped<IValidator<TwoFactorVerifyDto>, TwoFactorVerifyDtoValidator>();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IChatMessageService, ChatMessageService>();
        
        services.AddScoped<IGatewayHandler, GatewayHandler>();

        services.AddHostedService<GatewayHost>();

        return services;
    }
}