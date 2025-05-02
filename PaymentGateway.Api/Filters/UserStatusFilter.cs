using Microsoft.AspNetCore.Identity;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Api.Filters;

public class UserStatusFilter(UserManager<UserEntity> userManager) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        
        if (httpContext.Request.Path.Value?.Contains("/user/login", StringComparison.OrdinalIgnoreCase) == true)
        {
            return await next(context);
        }

        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var username = httpContext.User.Identity.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Results.Unauthorized();
            }

            var user = await userManager.FindByNameAsync(username);
            if (user is not { IsActive: true })
            {
                return Results.Unauthorized();
            }
        }

        return await next(context);
    }
}