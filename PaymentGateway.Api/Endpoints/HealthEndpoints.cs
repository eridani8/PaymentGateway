using Carter;
using PaymentGateway.Api.Filters;

namespace PaymentGateway.Api.Endpoints;

public class HealthEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/health")
            .WithTags("Health")
            .AddEndpointFilter<UserStatusFilter>();

        group.MapGet("/check-available", CheckAvailable)
            .WithName("CheckAvailable")
            .WithDescription("Проверка доступности сервиса")
            .WithSummary("Проверка доступности сервиса")
            .Produces(StatusCodes.Status200OK);
    }

    private static IResult CheckAvailable()
    {
        return Results.Ok();
    }
}