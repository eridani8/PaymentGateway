using System.Security.Claims;
using PaymentGateway.Application.Results;
using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Application.Interfaces;

public interface IWalletService
{
    Task<Result> Deposit(DepositDto dto, ClaimsPrincipal currentUser);
}