﻿using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Application.DTOs.Payment;
using PaymentGateway.Application.DTOs.Transaction;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Mappings;
using PaymentGateway.Application.Services;
using PaymentGateway.Application.Validators.Payment;
using PaymentGateway.Application.Validators.Transaction;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared.DTOs;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Validations;
using PaymentGateway.Shared.Validations.Validators;
using PaymentGateway.Shared.Validations.Validators.Requisite;
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
        services.AddScoped<IPaymentService, PaymentService>();

        services.AddScoped<IValidator<TransactionCreateDto>, TransactionCreateDtoValidator>();
        services.AddScoped<ITransactionService, TransactionService>();

        services.AddScoped<IValidator<LoginDto>, LoginModelValidator>();
        services.AddScoped<IValidator<ChangePasswordDto>, ChangePasswordValidator>();
        services.AddScoped<IValidator<CreateUserDto>, CreateUserValidator>();

        // services.AddScoped<IValidator<CreateUserDto>, CreateUserValidator>(); // TODO

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAdminService, AdminService>();
        
        services.AddScoped<IGatewayHandler, GatewayHandler>();

        services.AddHostedService<GatewayHost>();

        return services;
    }
}