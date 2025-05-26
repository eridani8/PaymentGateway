using FluentValidation;
using Microsoft.AspNetCore.Identity;
using PaymentGateway.Api.Filters;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared;
using PaymentGateway.Shared.DTOs.User;
using System.Security.Claims;
using Asp.Versioning;
using Carter;
using Microsoft.AspNetCore.Authorization;
using PaymentGateway.Application.Results;
using PaymentGateway.Shared.Services;

namespace PaymentGateway.Api.Endpoints;

public class UsersEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();
        
        var group = app.MapGroup("api/users")
            .WithApiVersionSet(versionSet)
            .WithTags("Пользовательские методы и аутентификация")
            .AddEndpointFilter<UserStatusFilter>();

        group.MapPost("/deposit", Deposit)
            .WithName("Deposit")
            .WithSummary("Пополнение баланса пользователя")
            .WithDescription("Добавляет средства на счет пользователя")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute() { Roles = "Admin" });
        
        group.MapPost("login", Login)
            .WithName("Login")
            .WithSummary("Аутентификация пользователя")
            .WithDescription("Выполняет вход пользователя в систему. При включенной двухфакторной аутентификации требует дополнительного кода.")
            .Produces<string>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status428PreconditionRequired)
            .AllowAnonymous();

        group.MapPut("password", ChangePassword)
            .WithName("ChangePassword")
            .WithSummary("Изменение пароля")
            .WithDescription("Позволяет пользователю изменить свой пароль")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute() { Roles = "User,Admin,Support" });

        group.MapGet("two-factor/status", TwoFactorStatus)
            .WithName("TwoFactorStatus")
            .WithSummary("Статус двухфакторной аутентификации")
            .WithDescription("Возвращает информацию о состоянии двухфакторной аутентификации пользователя")
            .Produces<TwoFactorStatusDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute() { Roles = "Admin,Support" });

        group.MapPost("two-factor/enable", EnableTwoFactor)
            .WithName("EnableTwoFactor")
            .WithSummary("Включение двухфакторной аутентификации")
            .WithDescription("Генерирует QR-код и секретный ключ для настройки двухфакторной аутентификации")
            .Produces<TwoFactorDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute() { Roles = "Admin,Support" });

        group.MapPost("two-factor/verify", VerifyTwoFactor)
            .WithName("VerifyTwoFactor")
            .WithSummary("Проверка кода двухфакторной аутентификации")
            .WithDescription("Проверяет введенный код двухфакторной аутентификации")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute() { Roles = "Admin,Support" });
    }

    private static async Task<IResult> Deposit(
        DepositDto? dto,
        IValidator<DepositDto> validator,
        UserManager<UserEntity> userManager,
        ILogger<UsersEndpoints> logger,
        ClaimsPrincipal currentUser)
    {
        if (dto is null) return Results.BadRequest();
        
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            return Results.BadRequest(validation.Errors.GetErrors());
        }
        
        var user = await userManager.FindByIdAsync(dto.UserId.ToString());
        if (user is not { IsActive: true })
        {
            return Results.NotFound(UserErrors.UserNotFound.ToString());
        }
        
        var oldBalance = user.Balance;
        
        user.Balance += dto.Amount;
        await userManager.UpdateAsync(user);
        
        logger.LogInformation("Пополнение счета пользователя {UserId} на {DepositAmount} [{CurrentUser}]. Было {OldBalance}, стало {NewBalance}", dto.UserId, dto.Amount, currentUser.GetCurrentUsername(), oldBalance, user.Balance);
        
        return Results.Ok();
    }
    
    private static async Task<IResult> Login(
        LoginDto? dto,
        UserManager<UserEntity> userManager,
        SignInManager<UserEntity> signInManager,
        ITokenService tokenService,
        IValidator<LoginDto> validator)
    {
        if (dto is null) return Results.BadRequest();

        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            return Results.BadRequest(validation.Errors.GetErrors());
        }
            
        var user = await userManager.FindByNameAsync(dto.Username);
        if (user is not { IsActive: true })
        {
            return Results.BadRequest(UserErrors.UserNotFound.ToString());
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
        {
            return Results.BadRequest(UserErrors.InappropriateData.ToString());
        }

        switch (user.TwoFactorEnabled)
        {
            case true when string.IsNullOrEmpty(dto.TwoFactorCode):
                return Results.StatusCode(StatusCodes.Status428PreconditionRequired);
            case true when !string.IsNullOrEmpty(dto.TwoFactorCode):
            {
                var isValid = TotpService.VerifyTotpCode(user.TwoFactorSecretKey ?? string.Empty, dto.TwoFactorCode);
                if (!isValid)
                {
                    return Results.BadRequest(UserErrors.InappropriateCode.ToString());
                }
                break;
            }
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = tokenService.GenerateJwtToken(user, roles);
        
        return Results.Ok(token);
    }
    
    private static async Task<IResult> ChangePassword(
        ChangePasswordDto? dto,
        UserManager<UserEntity> userManager,
        IValidator<ChangePasswordDto> changePasswordValidator,
        ClaimsPrincipal user)
    {
        if (dto is null) return Results.BadRequest();

        var validation = await changePasswordValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            return Results.BadRequest(validation.Errors.GetErrors());
        }
        
        var userEntity = await userManager.GetUserAsync(user);
        if (userEntity is null)
        {
            return Results.NotFound();
        }

        var result = await userManager.ChangePasswordAsync(
            userEntity,
            dto.CurrentPassword,
            dto.NewPassword);

        if (result.Succeeded)
        {
            return Results.Ok();
        }

        return Results.BadRequest(result.Errors.GetErrors());
    }
    
    private static async Task<IResult> TwoFactorStatus(
        UserManager<UserEntity> userManager,
        ClaimsPrincipal user)
    {
        var userEntity = await userManager.GetUserAsync(user);
        if (userEntity is null)
        {
            return Results.NotFound();
        }
        
        var roles = await userManager.GetRolesAsync(userEntity);
        
        var result = new TwoFactorStatusDto
        {
            IsEnabled = userEntity.TwoFactorEnabled,
            IsSetupRequired = roles.Contains("Admin") && !userEntity.TwoFactorEnabled
        };
        
        return Results.Json(result);
    }
    
    private static async Task<IResult> EnableTwoFactor(
        UserManager<UserEntity> userManager,
        ClaimsPrincipal user)
    {
        var userEntity = await userManager.GetUserAsync(user);
        if (userEntity is null)
        {
            return Results.NotFound();
        }
        
        var secretKey = TotpService.GenerateSecretKey();
        
        const string issuer = "PaymentGateway";
        var totpUri = TotpService.GenerateTotpUri(secretKey, userEntity.UserName ?? userEntity.Email ?? userEntity.Id.ToString(), issuer);
        
        var qrCodeImage = TotpService.GenerateQrCodeBase64(totpUri);
        
        userEntity.TwoFactorSecretKey = secretKey;
        await userManager.UpdateAsync(userEntity);
        
        return Results.Json(new TwoFactorDto
        {
            QrCodeUri = qrCodeImage,
            SharedKey = secretKey
        });
    }
    
    private static async Task<IResult> VerifyTwoFactor(
        TwoFactorVerifyDto? dto,
        UserManager<UserEntity> userManager,
        IValidator<TwoFactorVerifyDto> twoFactorVerifyValidator,
        ClaimsPrincipal user)
    {
        if (dto is null) return Results.BadRequest();
        
        var validation = await twoFactorVerifyValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            return Results.BadRequest(validation.Errors.GetErrors());
        }
        
        var userEntity = await userManager.GetUserAsync(user);
        if (userEntity is null)
        {
            return Results.NotFound();
        }
        
        var isValid = TotpService.VerifyTotpCode(userEntity.TwoFactorSecretKey ?? string.Empty, dto.Code);
        if (!isValid)
        {
            return Results.BadRequest(UserErrors.InappropriateCode.ToString());
        }
        
        userEntity.TwoFactorEnabled = true;
        await userManager.UpdateAsync(userEntity);
        
        return Results.Ok();
    }
}