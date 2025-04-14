using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Api.Filters;

// ReSharper disable once ClassNeverInstantiated.Global
public class UserStatusFilter(UserManager<UserEntity> userManager) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.Request.Path.Value?.Contains("/user/login", StringComparison.OrdinalIgnoreCase) == true)
        {
            return;
        }

        if (context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            var username = context.HttpContext.User.Identity.Name;
            if (string.IsNullOrEmpty(username))
            {
                context.Result = new UnauthorizedObjectResult("Пользователь не найден");
                return;
            }

            var user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                context.Result = new UnauthorizedObjectResult("Пользователь не найден");
                return;
            }

            if (!user.IsActive)
            {
                context.Result = new UnauthorizedObjectResult("Пользователь деактивирован");
            }
        }
    }
} 