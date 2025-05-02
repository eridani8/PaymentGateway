using Microsoft.AspNetCore.Authorization;
using PaymentGateway.Api.Filters;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Results;
using PaymentGateway.Shared.DTOs.Requisite;
using System.Security.Claims;

namespace PaymentGateway.Api.Endpoints;

public class RequisitesEndpoints
{
    public static void MapRequisitesEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/requisites")
            .WithTags("Управление реквизитами")
            .AddEndpointFilter<UserStatusFilter>()
            .RequireAuthorization();

        group.MapPost("/", CreateRequisite)
            .WithName("CreateRequisite")
            .WithSummary("Создание реквизита")
            .WithDescription("Создает новый реквизит для пользователя")
            .Produces<Guid>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/", GetAllRequisites)
            .WithName("GetAllRequisites")
            .WithSummary("Получение всех реквизитов")
            .WithDescription("Возвращает список всех реквизитов в системе (только для администраторов и поддержки)")
            .Produces<IEnumerable<RequisiteDto>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Support" });

        group.MapGet("/user", GetUserRequisites)
            .WithName("GetUserRequisites")
            .WithSummary("Получение реквизитов пользователя")
            .WithDescription("Возвращает список реквизитов текущего пользователя")
            .Produces<IEnumerable<RequisiteDto>>()
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", GetRequisiteById)
            .WithName("GetRequisiteById")
            .WithSummary("Получение реквизита по ID")
            .WithDescription("Возвращает информацию о реквизите по его идентификатору")
            .Produces<RequisiteDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateRequisite)
            .WithName("UpdateRequisite")
            .WithSummary("Обновление реквизита")
            .WithDescription("Обновляет информацию о реквизите")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteRequisite)
            .WithName("DeleteRequisite")
            .WithSummary("Удаление реквизита")
            .WithDescription("Удаляет реквизит по его идентификатору")
            .Produces<RequisiteDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateRequisite(
        RequisiteCreateDto? dto,
        IRequisiteService service,
        ILogger<RequisitesEndpoints> logger,
        ClaimsPrincipal user)
    {
        if (dto is null) return Results.BadRequest();

        var userId = user.GetCurrentUserId();
        if (userId == Guid.Empty) return Results.Unauthorized();
        
        var result = await service.CreateRequisite(dto, userId);
        
        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                ErrorCode.DuplicateRequisite => Results.Conflict(result.Error.Message),
                ErrorCode.RequisiteLimitExceeded => Results.BadRequest(result.Error.Message),
                ErrorCode.Validation => Results.BadRequest(result.Error.Message),
                ErrorCode.UserNotFound => Results.NotFound(result.Error.Message),
                _ => Results.BadRequest(result.Error.Message)
            };
        }
        
        logger.LogInformation("Создание реквизита {requisiteId} [{User}]", result.Value.Id, user.GetCurrentUsername());
        return Results.Ok(result.Value.Id);
    }
    
    private static async Task<IResult> GetAllRequisites(
        IRequisiteService service)
    {
        var result = await service.GetAllRequisites();
        
        if (result.IsFailure) return Results.BadRequest(result.Error.Message);
        
        return Results.Json(result.Value);
    }

    private static async Task<IResult> GetUserRequisites(
        IRequisiteService service,
        ClaimsPrincipal user)
    {
        var userId = user.GetCurrentUserId();
        if (userId == Guid.Empty) return Results.Unauthorized();
        
        var result = await service.GetUserRequisites(userId);
        
        if (result.IsFailure) return Results.BadRequest(result.Error.Message);
        
        return Results.Json(result.Value);
    }
    
    private static async Task<IResult> GetRequisiteById(
        Guid id,
        IRequisiteService service,
        ClaimsPrincipal user)
    {
        var userId = user.GetCurrentUserId();
        if (userId == Guid.Empty) return Results.Unauthorized();
        
        var result = await service.GetRequisiteById(id);
        
        if (result.IsFailure)
        {
            if (result.Error.Code == ErrorCode.RequisiteNotFound) return Results.NotFound(result.Error.Message);
            
            return Results.BadRequest(result.Error.Message);
        }
        
        return Results.Json(result.Value);
    }
    
    private static async Task<IResult> UpdateRequisite(
        Guid id,
        RequisiteUpdateDto? dto,
        IRequisiteService service,
        ILogger<RequisitesEndpoints> logger,
        ClaimsPrincipal user)
    {
        if (dto is null) return Results.BadRequest();
        
        var result = await service.UpdateRequisite(id, dto);
        
        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                ErrorCode.RequisiteNotFound => Results.NotFound(result.Error.Message),
                _ => Results.BadRequest(result.Error.Message)
            };
        }
        
        logger.LogInformation("Обновление реквизита {requisiteId} [{User}]", result.Value.Id, user.GetCurrentUsername());
        return Results.Ok();
    }
    
    private static async Task<IResult> DeleteRequisite(
        Guid id,
        IRequisiteService service,
        ILogger<RequisitesEndpoints> logger,
        ClaimsPrincipal user)
    {
        var result = await service.DeleteRequisite(id);
        
        if (result.IsFailure)
        {
            if (result.Error.Code == ErrorCode.RequisiteNotFound) return Results.NotFound(result.Error.Message);
            
            return Results.BadRequest(result.Error.Message);
        }
        
        logger.LogInformation("Удаление реквизита {requisiteId} [{User}]", result.Value.Id, user.GetCurrentUsername());
        return Results.Ok(result.Value);
    }
} 