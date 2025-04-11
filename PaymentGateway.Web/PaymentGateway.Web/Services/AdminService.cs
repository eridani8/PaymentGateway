using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class AdminService(IHttpClientFactory factory, ILogger<AdminService> logger) : ServiceBase(factory, logger), IAdminService
{
    private const string ApiEndpoint = "users";
    
    public async Task<List<UserResponseDto>> GetUsers()
    {
        var users = await CreateRequest<List<UserResponseDto>>($"{ApiEndpoint}/GetAllUsers");
        return users ?? [];
    }
}