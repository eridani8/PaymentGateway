using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Types;

namespace PaymentGateway.Web.Interfaces;

public interface IUserService
{
    Task<WalletDto?> GetWalletState();
    Task<Response> Login(LoginDto dto);
    Task<Response> ChangePassword(ChangePasswordDto dto);
    Task<Response<TwoFactorStatusDto>> GetTwoFactorStatus();
    Task<Response<TwoFactorDto>> EnableTwoFactor();
    Task<Response> VerifyTwoFactor(TwoFactorVerifyDto dto);
    Task<decimal> GetCurrentUsdtExchangeRate();
}