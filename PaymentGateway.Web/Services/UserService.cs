using Microsoft.AspNetCore.Components.Authorization;
using PaymentGateway.Shared.DTOs;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class UserService(
    IHttpClientFactory factory,
    ILogger<UserService> logger) : ServiceBase(factory, logger), IUserService
{
    public Task<Response> Login(LoginDto dto)
    {
        return CreateRequest("user/login", dto);
    }

    public Task<Response> ChangePasswordAsync(ChangePasswordDto dto)
    {
        return CreateRequest("user/changepassword", dto);
    }
}