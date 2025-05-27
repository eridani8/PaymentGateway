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
using PaymentGateway.Application.Extensions;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Interfaces;
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
        
        group.MapGet("/", GetDevices)
            .WithName("GetAllDevices")
            .WithSummary("Получение всех устройств")
            .WithDescription("Возвращает список всех устройств администратору")
            .Produces<List<DeviceDto>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .AddEndpointFilter<UserStatusFilter>()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        
        group.MapGet("/user/{userId:guid}", GetDevicesByUserId)
            .WithName("GetDevicesByUserId")
            .WithSummary("Получение устройств пользователя по его ID")
            .WithDescription("Возвращает список устройств пользователя")
            .Produces<List<DeviceDto>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .AddEndpointFilter<UserStatusFilter>()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        
        group.MapGet("/user", GetUserDevices)
            .WithName("GetUserDevices")
            .WithSummary("Получение устройств текущего пользователя")
            .WithDescription("Возвращает список устройств текущему пользователю")
            .Produces<List<DeviceDto>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .AddEndpointFilter<UserStatusFilter>()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "User" });

        group.MapPost("/user/token", GenerateDeviceToken)
            .WithName("GenerateDeviceToken")
            .WithSummary("Генерация токена для привязки устройства")
            .WithDescription("Возвращает QR код и токен для привязки устройства")
            .Produces<DeviceTokenDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .AddEndpointFilter<UserStatusFilter>()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "User,Admin,Support" });
        
        group.MapDelete("/{deviceId:guid}", RemoveDevice)
            .WithName("RemoveDevice")
            .WithSummary("Удаление устройства")
            .WithDescription("Удаляет непривязанное оффлайн устройство")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }

    private static IResult GetDevices()
    {
        var devices = DeviceHub.Devices.Values.Reverse().ToList();
        
        return Results.Json(devices);
    }

    private static IResult GetDevicesByUserId(Guid userId)
    {
        var devices = DeviceHub.AvailableDevicesByUserId(userId);

        return Results.Json(devices);
    }
    
    private static IResult GetUserDevices(ClaimsPrincipal user, bool onlyAvailable = false, bool onlyOnline = false)
    {
        var currentUserId = user.GetCurrentUserId();
        if (currentUserId == Guid.Empty) return Results.Unauthorized();

        var devices = DeviceHub.UserDevices(currentUserId);

        if (onlyAvailable)
        {
            devices = devices.Where(d => d.BindingAt == DateTime.MinValue);
        }

        if (onlyOnline)
        {
            devices = devices.Where(d => d.State);
        }

        return Results.Json(devices.ToList());
    }

    private static async Task<IResult> GenerateDeviceToken(
        ITokenService tokenService,
        UserManager<UserEntity> userManager,
        ClaimsPrincipal user)
    {
        var userEntity = await userManager.GetUserAsync(user);
        if (userEntity is null)
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
            .OrderBy(r => roleOrder.GetValueOrDefault(r, int.MaxValue))
            .First();
        var token = tokenService.GenerateDeviceJwtToken(userEntity, role);
        var qrCodeUri = TotpService.GenerateQrCodeBase64(token);

        return Results.Json(new DeviceTokenDto
        {
            Token = token,
            QrCodeUri = qrCodeUri
        });
    }

    private static async Task<IResult> RemoveDevice(Guid deviceId, IUnitOfWork unit, INotificationService notificationService)
    {
        var device = await unit.DeviceRepository.GetDeviceById(deviceId);
        if (device is null)
        {
            return Results.NotFound();
        }
        
        var deviceDto = DeviceHub.Devices.Values.FirstOrDefault(d => d.Id == deviceId && d.BindingAt == DateTime.MinValue && d.State == false);
        if (deviceDto is null)
        {
            return Results.BadRequest("Устройство должно быть оффлайн и не привязано к реквизиту");
        }

        unit.DeviceRepository.Delete(device);
        await unit.Commit();
        
        DeviceHub.Devices.TryRemove(deviceDto.Id, out _);
        
        await notificationService.NotifyDeviceDeleted(deviceDto.Id, deviceDto.UserId);

        return Results.Ok();
    }
}