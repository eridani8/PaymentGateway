using Asp.Versioning;
using Carter;
using Microsoft.Extensions.Caching.Memory;
using PaymentGateway.Api.Filters;
using PaymentGateway.Application.Interfaces;

namespace PaymentGateway.Api.Endpoints;

public class DeviceEndpoints() : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup("api/v{version:apiVersion}/device")
            .WithApiVersionSet(versionSet)
            .WithTags("Взаимодействие с мобильным приложением")
            .AddEndpointFilter<UserStatusFilter>();

        group.MapPost("/pong", Pong)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/add", Add)
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/remove", Remove)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Pong(Guid code, IDeviceService deviceService)
    {
        if (code == Guid.Empty) return Results.BadRequest();

        await deviceService.Pong(code);
        
        return Results.Ok();
    }

    private static IResult Add()
    {
        return Results.Ok();
    }

    private static IResult Remove()
    {
        return Results.Ok();
    }
}