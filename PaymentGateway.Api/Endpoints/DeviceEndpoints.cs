using Asp.Versioning;
using Carter;
using Microsoft.AspNetCore.Authorization;
using PaymentGateway.Api.Filters;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Types;
using PaymentGateway.Shared.DTOs.Device;

namespace PaymentGateway.Api.Endpoints;

public class DeviceEndpoints() : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup("api/device")
            .WithApiVersionSet(versionSet)
            .WithTags("Взаимодействие с мобильным приложением")
            .AddEndpointFilter<UserStatusFilter>();
        
        group.MapPost("/pong", Pong)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/bind", Bind)
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/unbind", Unbind)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);
        
        group.MapGet("/", GetAllDevices)
            .Produces<List<DeviceDto>>()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .AddEndpointFilter<UserStatusFilter>()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }

    private static IResult Pong(PingDto dto, IDeviceService deviceService)
    {
        var result = deviceService.Pong(dto);

        if (result.IsFailure)
        {
            return Results.BadRequest(result.Error.Message);
        }
        
        return Results.Ok();
    }

    private static IResult Bind()
    {
        return Results.Ok();
    }

    private static IResult Unbind()
    {
        return Results.Ok();
    }

    private static IResult GetAllDevices(IDeviceService deviceService)
    {
        return Results.Json(deviceService.GetAvailableDevices());
    }
}