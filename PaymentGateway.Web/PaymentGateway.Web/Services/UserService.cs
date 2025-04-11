using PaymentGateway.Shared.Models;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class UserService(IHttpClientFactory factory, ILogger<UserService> logger) : ServiceBase(factory, logger), IUserService
{
    public Task<Response> Login(LoginModel model)
    {
        return CreateRequest("user/login", model);
    }

    public Task<Response> ChangePasswordAsync(ChangePasswordModel model)
    {
        return CreateRequest("user/changepassword", model);
    }
}