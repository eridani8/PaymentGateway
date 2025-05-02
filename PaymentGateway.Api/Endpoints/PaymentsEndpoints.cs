using Microsoft.AspNetCore.Authorization;
using PaymentGateway.Api.Filters;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Results;
using PaymentGateway.Shared.DTOs.Payment;
using System.Security.Claims;

namespace PaymentGateway.Api.Endpoints;

public class PaymentsEndpoints
{
    public static void MapPaymentsEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/payments")
            .WithTags("Управление платежами")
            .AddEndpointFilter<UserStatusFilter>();

        group.MapPost("/", CreatePayment)
            .WithName("CreatePayment")
            .WithSummary("Создание платежа")
            .WithDescription("Создает новый платеж в системе")
            .Produces<Guid>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPut("confirm", ManualConfirmPayment)
            .WithName("ManualConfirmPayment")
            .WithSummary("Ручное подтверждение платежа")
            .WithDescription("Подтверждает платеж вручную")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapPut("cancel", CancelPayment)
            .WithName("CancelPayment")
            .WithSummary("Отмена платежа")
            .WithDescription("Отменяет платеж (только для администраторов и поддержки)")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Support" });

        group.MapGet("/", GetAllPayments)
            .WithName("GetAllPayments")
            .WithSummary("Получение всех платежей")
            .WithDescription("Возвращает список всех платежей в системе (только для администраторов и поддержки)")
            .Produces<IEnumerable<PaymentDto>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Support" });

        group.MapGet("user", GetUserPayments)
            .WithName("GetUserPayments")
            .WithSummary("Получение платежей пользователя")
            .WithDescription("Возвращает список платежей текущего пользователя")
            .Produces<IEnumerable<PaymentDto>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapGet("{id:guid}", GetPaymentById)
            .WithName("GetPaymentById")
            .WithSummary("Получение платежа по ID")
            .WithDescription("Возвращает информацию о платеже по его идентификатору")
            .Produces<PaymentDto>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("{id:guid}", DeletePayment)
            .WithName("DeletePayment")
            .WithSummary("Удаление платежа")
            .WithDescription("Удаляет платеж по его идентификатору (только для администраторов и поддержки)")
            .Produces<PaymentDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Support" });
    }

    private static async Task<IResult> CreatePayment(
        PaymentCreateDto? dto,
        IPaymentService service,
        ILogger<PaymentsEndpoints> logger)
    {
        if (dto is null) return Results.BadRequest();

        var result = await service.CreatePayment(dto);
        
        if (result.IsFailure)
        {
            if (result.Error.Code == ErrorCode.DuplicatePayment) return Results.Conflict(result.Error.Message);
                
            return Results.BadRequest(result.Error.Message);
        }
        
        logger.LogInformation("Создание платежа {paymentId} на сумму {amount}", result.Value.Id, result.Value.Amount);
        return Results.Ok(result.Value.Id);
    }

    private static async Task<IResult> ManualConfirmPayment(
        PaymentManualConfirmDto? dto,
        IPaymentService service,
        ILogger<PaymentsEndpoints> logger,
        ClaimsPrincipal user)
    {
        if (dto is null) return Results.BadRequest();

        var userId = user.GetCurrentUserId();
        if (userId == Guid.Empty) return Results.Unauthorized();
        
        var result = await service.ManualConfirmPayment(dto, userId);
        
        if (result.IsFailure)
        {
            if (result.Error.Code == ErrorCode.PaymentNotFound) return Results.NotFound(result.Error.Message);
                
            return Results.BadRequest(result.Error.Message);
        }
        
        logger.LogInformation("Ручное подтверждение платежа {paymentId} [{User}]", result.Value.Id, user.GetCurrentUsername());
        return Results.Ok();
    }
    
    private static async Task<IResult> CancelPayment(
        PaymentCancelDto? dto,
        IPaymentService service,
        ILogger<PaymentsEndpoints> logger,
        ClaimsPrincipal user)
    {
        if (dto is null) return Results.BadRequest();

        var userId = user.GetCurrentUserId();
        if (userId == Guid.Empty) return Results.Unauthorized();
        
        var result = await service.CancelPayment(dto, userId);
        
        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                ErrorCode.PaymentNotFound => Results.NotFound(result.Error.Message),
                ErrorCode.InsufficientPermissions => Results.Forbid(),
                _ => Results.BadRequest(result.Error.Message)
            };
        }
        
        logger.LogInformation("Отмена платежа {paymentId} [{User}]", result.Value.Id, user.GetCurrentUsername());
        return Results.Ok();
    }
    
    private static async Task<IResult> GetAllPayments(IPaymentService service)
    {
        var result = await service.GetAllPayments();
        
        if (result.IsFailure) return Results.BadRequest(result.Error.Message);
            
        return Results.Json(result.Value);
    }

    private static async Task<IResult> GetUserPayments(
        IPaymentService service,
        ClaimsPrincipal user)
    {
        var userId = user.GetCurrentUserId();
        if (userId == Guid.Empty) return Results.Unauthorized();
        
        var result = await service.GetUserPayments(userId);
        
        if (result.IsFailure) return Results.BadRequest(result.Error.Message);
            
        return Results.Json(result.Value);
    }
    
    private static async Task<IResult> GetPaymentById(
        Guid id,
        IPaymentService service)
    {
        var result = await service.GetPaymentById(id);
        
        if (result.IsFailure)
        {
            if (result.Error.Code == ErrorCode.PaymentNotFound) return Results.NotFound(result.Error.Message);
                
            return Results.BadRequest(result.Error.Message);
        }
        
        return Results.Json(result.Value);
    }
    
    private static async Task<IResult> DeletePayment(
        Guid id,
        IPaymentService service,
        ILogger<PaymentsEndpoints> logger,
        ClaimsPrincipal user)
    {
        var result = await service.DeletePayment(id);
        
        if (result.IsFailure)
        {
            if (result.Error.Code == ErrorCode.PaymentNotFound) return Results.NotFound(result.Error.Message);
                
            return Results.BadRequest(result.Error.Message);
        }
        
        logger.LogInformation("Удаление платежа {paymentId} [{UserId}]", result.Value.Id, user.GetCurrentUserId());
        return Results.Ok(result.Value);
    }
} 