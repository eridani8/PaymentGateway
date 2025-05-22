using Microsoft.AspNetCore.Authorization;
using PaymentGateway.Api.Filters;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Results;
using PaymentGateway.Shared.DTOs.Transaction;
using System.Security.Claims;
using Asp.Versioning;
using Carter;

namespace PaymentGateway.Api.Endpoints;

public class TransactionsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup("api/transactions")
            .WithApiVersionSet(versionSet)
            .WithTags("Управление транзакциями")
            .AddEndpointFilter<UserStatusFilter>();

        group.MapPost("/", CreateTransaction)
            .WithName("CreateTransaction")
            .WithSummary("Создание транзакции")
            .WithDescription("Создает новую транзакцию в системе")
            .Produces<TransactionDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/", GetAllTransactions)
            .WithName("GetAllTransactions")
            .WithSummary("Получение всех транзакций")
            .WithDescription("Возвращает список всех транзакций в системе (только для администраторов и поддержки)")
            .Produces<List<TransactionDto>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Support" });

        group.MapGet("/user/{userId:guid}", GetTransactionsByUserId)
            .WithName("GetTransactionsByUserId")
            .WithSummary("Получение транзакций пользователя")
            .WithDescription("Возвращает список транзакций пользователя")
            .Produces<List<TransactionDto>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Support" });

        group.MapGet("user", GetUserTransactions)
            .WithName("GetUserTransactions")
            .WithSummary("Получение транзакций пользователя")
            .WithDescription("Возвращает список транзакций текущего пользователя")
            .Produces<List<TransactionDto>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization();
    }

    private static async Task<IResult> CreateTransaction(
        TransactionCreateDto? dto,
        ITransactionService service,
        ILogger<TransactionsEndpoints> logger)
    {
        if (dto is null) return Results.BadRequest();

        var result = await service.CreateTransaction(dto);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                ErrorCode.RequisiteNotFound => Results.NotFound(result.Error.Message),
                _ => Results.BadRequest(result.Error.Message)
            };
        }

        logger.LogInformation("Создание транзакции {transactionId} на сумму {amount}", result.Value.Id,
            result.Value.ExtractedAmount);
        return Results.Created($"/api/transactions/{result.Value.Id}", result.Value);
    }

    private static async Task<IResult> GetAllTransactions(
        ITransactionService service)
    {
        var result = await service.GetAllTransactions();

        if (result.IsFailure)
        {
            return Results.BadRequest(result.Error.Message);
        }

        return Results.Json(result.Value);
    }

    private static async Task<IResult> GetTransactionsByUserId(Guid userId, ITransactionService service)
    {
        var result = await service.GetUserTransactions(userId);

        if (result.IsFailure) return Results.BadRequest(result.Error.Message);
        
        return Results.Json(result.Value);
    }

    private static async Task<IResult> GetUserTransactions(
        ITransactionService service,
        ClaimsPrincipal user)
    {
        var userId = user.GetCurrentUserId();
        if (userId == Guid.Empty) return Results.Unauthorized();

        var result = await service.GetUserTransactions(userId);

        if (result.IsFailure)
        {
            return Results.BadRequest(result.Error.Message);
        }

        return Results.Json(result.Value);
    }
}