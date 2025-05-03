using Microsoft.AspNetCore.Authorization;
using PaymentGateway.Api.Filters;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Results;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Enums;
using PaymentGateway.Shared.Interfaces;
using System.Security.Claims;
using Carter;
using Asp.Versioning;

namespace PaymentGateway.Api.Endpoints;

public class AdminEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup("api/v{version:apiVersion}/admin")
            .WithApiVersionSet(versionSet)
            .WithTags("Административные методы управления пользователями и системой")
            .AddEndpointFilter<UserStatusFilter>()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        group.MapPost("users", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Создание нового пользователя")
            .WithDescription("Создает нового пользователя в системе")
            .Produces<Guid>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status409Conflict);

        group.MapGet("users", GetAllUsers)
            .WithName("GetAllUsers")
            .WithSummary("Получение списка всех пользователей")
            .WithDescription("Возвращает список всех пользователей системы")
            .Produces<List<UserDto>>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("users/{id:guid}", GetUserById)
            .WithName("GetUserById")
            .WithSummary("Получение пользователя по ID")
            .WithDescription("Возвращает информацию о пользователе по его идентификатору")
            .Produces<UserDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("users/{id:guid}", DeleteUser)
            .WithName("DeleteUser")
            .WithSummary("Удаление пользователя")
            .WithDescription("Удаляет пользователя из системы по его идентификатору")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("users", UpdateUser)
            .WithName("UpdateUser")
            .WithSummary("Обновление данных пользователя")
            .WithDescription("Обновляет информацию о пользователе")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("users/roles", GetUsersRoles)
            .WithName("GetUsersRoles")
            .WithSummary("Получение ролей пользователей")
            .WithDescription("Возвращает роли для списка пользователей по их идентификаторам")
            .Produces<Dictionary<Guid, string>>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("users/{userId:guid}/reset-2fa", ResetTwoFactor)
            .WithName("ResetTwoFactor")
            .WithSummary("Сброс двухфакторной аутентификации")
            .WithDescription("Сбрасывает настройки двухфакторной аутентификации для указанного пользователя")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("requisite-assignment-algorithm", GetCurrentRequisiteAssignmentAlgorithm)
            .WithName("GetCurrentRequisiteAssignmentAlgorithm")
            .WithSummary("Получение текущего алгоритма назначения реквизитов")
            .WithDescription("Возвращает текущий алгоритм, используемый для назначения реквизитов")
            .Produces<int>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("requisite-assignment-algorithm", SetRequisiteAssignmentAlgorithm)
            .WithName("SetRequisiteAssignmentAlgorithm")
            .WithSummary("Изменение алгоритма назначения реквизитов")
            .WithDescription("Устанавливает новый алгоритм для назначения реквизитов")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> CreateUser(
        CreateUserDto? dto,
        IAdminService service,
        ILogger<AdminEndpoints> logger,
        ClaimsPrincipal user)
    {
        if (dto is null) return Results.BadRequest();

        var result = await service.CreateUser(dto);
        
        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                ErrorCode.Validation => Results.BadRequest(result.Error.Message),
                ErrorCode.UserAlreadyExists => Results.Conflict(result.Error.Message),
                _ => Results.BadRequest(result.Error.Message)
            };
        }
        
        logger.LogInformation("Создание пользователя {username} [{User}]", dto.Username, user.GetCurrentUsername());
        return Results.Ok(result.Value.Id);
    }

    private static async Task<IResult> GetAllUsers(
        IAdminService service)
    {
        var result = await service.GetAllUsers();
        
        if (result.IsFailure)
        {
            return Results.BadRequest(result.Error.Message);
        }
        
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetUserById(
        Guid id,
        IAdminService service)
    {
        var result = await service.GetUserById(id);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                ErrorCode.UserNotFound => Results.NotFound(result.Error.Message),
                _ => Results.BadRequest(result.Error.Message)
            };
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> DeleteUser(
        Guid id,
        IAdminService service,
        ILogger<AdminEndpoints> logger,
        ClaimsPrincipal user)
    {
        var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await service.DeleteUser(id, currentUserId);
        
        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                ErrorCode.UserNotFound => Results.NotFound(result.Error.Message),
                _ => Results.BadRequest(result.Error.Message)
            };
        }
        
        logger.LogInformation("Удаление пользователя {username} [{User}]", result.Value.UserName,
            user.GetCurrentUsername());
        return Results.Ok();
    }

    private static async Task<IResult> UpdateUser(
        UpdateUserDto? dto,
        IAdminService service,
        ILogger<AdminEndpoints> logger,
        ClaimsPrincipal user)
    {
        if (dto is null) return Results.BadRequest();

        var result = await service.UpdateUser(dto);
        
        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                ErrorCode.Validation => Results.BadRequest(result.Error.Message),
                ErrorCode.UserNotFound => Results.NotFound(result.Error.Message),
                _ => Results.BadRequest(result.Error.Message)
            };
        }
        
        logger.LogInformation("Обновление пользователя {username} [{User}]", result.Value.Username,
            user.GetCurrentUsername());
        return Results.Ok();
    }

    private static async Task<IResult> GetUsersRoles(
        string userIds,
        IAdminService service)
    {
        var ids = userIds.Split(',')
            .Select(Guid.Parse)
            .ToList();

        var result = await service.GetUsersRoles(ids);
            
        if (result.IsFailure)
        {
            return Results.BadRequest(result.Error.Message);
        }
            
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> ResetTwoFactor(
        Guid userId,
        IAdminService service,
        ILogger<AdminEndpoints> logger,
        ClaimsPrincipal user)
    {
        var result = await service.ResetTwoFactor(userId);
        
        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                ErrorCode.UserNotFound => Results.NotFound(result.Error.Message),
                _ => Results.BadRequest(result.Error.Message)
            };
        }
        
        logger.LogInformation("Сброс двухфакторной аутентификации для пользователя {userId} [{User}]", userId,
            user.GetCurrentUsername());
        return Results.Ok();
    }
    
    private static IResult GetCurrentRequisiteAssignmentAlgorithm(
        IAdminService service)
    {
        var result = service.GetCurrentRequisiteAssignmentAlgorithm();
        
        if (result.IsFailure)
        {
            return Results.BadRequest(result.Error.Message);
        }
        
        return Results.Ok(result.Value);
    }

    private static IResult SetRequisiteAssignmentAlgorithm(
        int algorithm,
        IAdminService service,
        INotificationService notificationService,
        ILogger<AdminEndpoints> logger,
        ClaimsPrincipal user)
    {
        var result = service.SetRequisiteAssignmentAlgorithm(algorithm);
        
        if (result.IsFailure)
        {
            return Results.BadRequest(result.Error.Message);
        }

        var oldAlgorithm = (RequisiteAssignmentAlgorithm)service.GetCurrentRequisiteAssignmentAlgorithm().Value;
        notificationService.NotifyRequisiteAssignmentAlgorithmChanged((RequisiteAssignmentAlgorithm)algorithm);
        
        logger.LogInformation("Изменение алгоритма подбора реквизитов. С {old} на {new} [{User}]", 
            oldAlgorithm, (RequisiteAssignmentAlgorithm)algorithm, user.GetCurrentUsername());

        return Results.Ok();
    }
} 