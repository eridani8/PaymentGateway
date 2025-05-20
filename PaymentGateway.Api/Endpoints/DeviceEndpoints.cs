using Asp.Versioning;
using Carter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using PaymentGateway.Api.Filters;
using PaymentGateway.Application.Hubs;
using PaymentGateway.Core.Entities;
using PaymentGateway.Shared;
using PaymentGateway.Shared.DTOs.Device;
using System.Security.Claims;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared.Services;

namespace PaymentGateway.Api.Endpoints;

public class DeviceEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup("api/device")
            .WithApiVersionSet(versionSet)
            .WithTags("Взаимодействие с устройствами")
            .AddEndpointFilter<UserStatusFilter>();
        
        group.MapGet("/", GetAllDevices)
            .Produces<List<DeviceDto>>()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .AddEndpointFilter<UserStatusFilter>()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        group.MapPost("/token", GenerateDeviceToken)
            .Produces<DeviceTokenDto>()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .AddEndpointFilter<UserStatusFilter>()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "User,Admin,Support" });
    }

    private IResult GetAllDevices()
    {
        var devices = DeviceHub.ConnectedDevices.Values.ToList();
        
        return Results.Json(devices);
    }

    private static async Task<IResult> GenerateDeviceToken(
        ITokenService tokenService,
        UserManager<UserEntity> userManager,
        ClaimsPrincipal user)
    {
        var userEntity = await userManager.GetUserAsync(user);
        if (userEntity == null)
        {
            return Results.Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(userEntity);
        var roleOrder = new Dictionary<string, int>
        {
            ["Admin"] = 0,
            ["Support"] = 1,
            ["User"] = 2
        };
        var role = roles
            .OrderBy(role => roleOrder.TryGetValue(role, out var order) ? order : int.MaxValue)
            .First();
        var token = tokenService.GenerateDeviceJwtToken(userEntity, role);
        var qrCodeUri = TotpService.GenerateQrCodeBase64(token);

        return Results.Json(new DeviceTokenDto
        {
            Token = token,
            QrCodeUri = qrCodeUri
        });
    }
}