using System.Net;
using PaymentGateway.Shared.Models;
using PaymentGateway.Web.Services;

namespace PaymentGateway.Web.Interfaces;

public interface IUserService
{
    Task<Response> Login(LoginModel model);
    Task<Response> ChangePasswordAsync(ChangePasswordModel model);
}