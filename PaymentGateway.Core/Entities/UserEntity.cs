using Microsoft.AspNetCore.Identity;

namespace PaymentGateway.Core.Entities;

public class UserEntity : IdentityUser<Guid>
{
    public List<string> Roles { get; } = [];
}