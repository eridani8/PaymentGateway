using Asp.Versioning;
using Carter;
using PaymentGateway.Api.Filters;
using PaymentGateway.Shared.DTOs.Device;

namespace PaymentGateway.Api.Endpoints;

public class DeviceEndpoints : ICarterModule
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

        group.MapPost("/reception-code", ReceptionCode)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
        
        group.MapPost("/ping", Ping)
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

    private static IResult ReceptionCode(ReceptionCodeDto dto, ILogger<DeviceEndpoints> logger)
    {
        logger.LogInformation("ReceptionCode: {Code}", dto.DeviceCode);
        return Results.Ok();
    }

    private static IResult Ping()
    {
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