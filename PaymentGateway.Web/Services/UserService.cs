using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class UserService(
    IHttpClientFactory factory,
    ILogger<UserService> logger) : ServiceBase(factory, logger), IUserService
{
    private const string ApiEndpoint = "user";
    
    public Task<Response> Login(LoginDto dto)
    {
        return PostRequest($"{ApiEndpoint}/login", dto);
    }

    public Task<Response> ChangePasswordAsync(ChangePasswordDto dto)
    {
        return PostRequest($"{ApiEndpoint}/ChangePassword", dto);
    }
}