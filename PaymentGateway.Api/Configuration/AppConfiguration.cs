using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PaymentGateway.Application;
using PaymentGateway.Application.Hubs;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Configs;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Interfaces;
using PaymentGateway.Shared.DTOs.Device;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Api.Configuration;

public static class AppConfiguration
{
    private static readonly string[] Roles = ["Admin", "User", "Support"];
    
    public static async Task InitializeApplication(WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserEntity>>();
        
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        await CreateUser(userManager, "root");
        await CreateUser(userManager, "eridani");
        
        var deviceRepository = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var devices = await deviceRepository.GetAllDevices();
        
        foreach (var device in devices)
        {
            var deviceDto = mapper.Map<DeviceDto>(device);
            deviceDto.User = mapper.Map<UserDto>(device.User);
            DeviceHub.Devices.TryAdd(device.Id, deviceDto);
        }
        
        var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
        var gatewayConfig = scope.ServiceProvider.GetRequiredService<GatewaySettings>();

        if (await settingsService.GetValue<RequisiteAssignmentAlgorithm>(nameof(gatewayConfig.AppointmentAlgorithm)) is { } requisiteAssignmentAlgorithm)
        {
            gatewayConfig.AppointmentAlgorithm = requisiteAssignmentAlgorithm;
        }

        if (await settingsService.GetValue<decimal>(nameof(gatewayConfig.UsdtExchangeRate)) is { } usdtExchangeRate)
        {
            gatewayConfig.UsdtExchangeRate = usdtExchangeRate;
        }
    }

    private static async Task CreateUser(UserManager<UserEntity> userManager, string username)
    {
        var defaultUser = await userManager.FindByNameAsync(username);
        if (defaultUser is null)
        {
            defaultUser = new UserEntity
            {
                Id = Guid.CreateVersion7(),
                UserName = username,
                IsActive = true,
                MaxRequisitesCount = 100,
                MaxDailyMoneyReceptionLimit = 1000000,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(defaultUser, "Qwerty123_");
            if (!result.Succeeded)
            {
                throw new ApplicationException($"Ошибка при создании пользователя {username}");
            }

            foreach (var role in Roles)
            {
                if (!await userManager.IsInRoleAsync(defaultUser, role))
                {
                    await userManager.AddToRoleAsync(defaultUser, role);
                }
            }
        }
    }
}