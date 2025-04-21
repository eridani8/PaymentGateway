using System.Security.Claims;

namespace PaymentGateway.Application;

public static class UserExtensions
{
    public static Guid GetCurrentUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Guid.Empty;
        }
        return userId;
    }
    
    public static string GetCurrentUsername(this ClaimsPrincipal user)
    {
        var usernameClaim = user.FindFirst(ClaimTypes.Name);
        return string.IsNullOrEmpty(usernameClaim?.Value) ? "NULL" : usernameClaim.Value;
    }
}