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
        if (context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            var user = await userManager.FindByNameAsync(context.HttpContext.User.Identity.Name!);
            if (user is { IsActive: false })
            {
                context.Result = new UnauthorizedObjectResult("Пользователь деактивирован");
            }
        }
    }
} 