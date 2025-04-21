using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Web.Services;

namespace PaymentGateway.Web.Interfaces;

public interface IUserService
{
    Task<Response> Login(LoginDto dto);
    Task<Response> ChangePasswordAsync(ChangePasswordDto dto);
    Task<Response<TwoFactorStatusDto>> GetTwoFactorStatusAsync();
    Task<Response<TwoFactorDto>> EnableTwoFactorAsync();
    Task<Response> VerifyTwoFactorAsync(TwoFactorVerifyDto dto);
}