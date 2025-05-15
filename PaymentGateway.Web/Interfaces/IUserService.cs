using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Types;
using PaymentGateway.Web.Services;

namespace PaymentGateway.Web.Interfaces;

public interface IUserService
{
    Task<Response> Login(LoginDto dto);
    Task<Response> ChangePassword(ChangePasswordDto dto);
    Task<Response<TwoFactorStatusDto>> GetTwoFactorStatus();
    Task<Response<TwoFactorDto>> EnableTwoFactor();
    Task<Response> VerifyTwoFactor(TwoFactorVerifyDto dto);
}