using Microsoft.AspNetCore.Identity;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Api.Middleware;

public class UserStatusMiddleware(
    RequestDelegate next,
    UserManager<UserEntity> userManager)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var user = await userManager.FindByNameAsync(context.User.Identity.Name!);
            if (user is { IsActive: false })
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Пользователь деактивирован");
                return;
            }
        }

        await next(context);
    }
} 