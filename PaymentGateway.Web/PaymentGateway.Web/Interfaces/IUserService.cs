using System.Net;
using PaymentGateway.Shared.Models;

namespace PaymentGateway.Web.Interfaces;

public interface IUserService
{
    Task<(HttpStatusCode, string?)> Login(LoginModel model);
    Task<(HttpStatusCode, string?)> ChangePasswordAsync(ChangePasswordModel model);
}