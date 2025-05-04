using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using PaymentGateway.Application.Hubs;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Api.Configuration;

public static class AppConfiguration
{
    private static readonly string[] Roles = ["Admin", "User", "Support"];
    
    public static async Task InitializeApplication(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub, IHubClient>>();
        NotificationHub.Initialize(hubContext);

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
    }

    private static async Task CreateUser(UserManager<UserEntity> userManager, string username)
    {
        var defaultUser = await userManager.FindByNameAsync(username);
        if (defaultUser == null)
        {
            defaultUser = new UserEntity
            {
                Id = Guid.CreateVersion7(),
                UserName = username,
                IsActive = true,
                MaxRequisitesCount = int.MaxValue,
                MaxDailyMoneyReceptionLimit = 9999999999999999m,
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