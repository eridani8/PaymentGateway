using System.Security.Claims;

namespace PaymentGateway.Api;

public static class ClaimsPrincipalExtensions
{
    public static string GetCurrentUsername(this ClaimsPrincipal principal)
    {
        return principal.Identity?.Name ?? "Unknown";
    }
    
    public static Guid GetCurrentUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Guid.Empty;
        }
        return userId;
    }
} 