using System.Net;
using PaymentGateway.Shared.DTOs;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Web.Services;

namespace PaymentGateway.Web.Interfaces;

public interface IUserService
{
    Task<Response> Login(LoginDto dto);
    Task<Response> ChangePasswordAsync(ChangePasswordDto dto);
}