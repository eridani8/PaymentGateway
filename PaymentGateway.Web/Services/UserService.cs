using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Web.Interfaces;
using System.Text.Json;

namespace PaymentGateway.Web.Services;

public class UserService(
    IHttpClientFactory factory,
    ILogger<UserService> logger,
    JsonSerializerOptions jsonOptions) : ServiceBase(factory, logger, jsonOptions), IUserService
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