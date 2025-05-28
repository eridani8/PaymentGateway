using PaymentGateway.Application.Results;
using PaymentGateway.Core.Entities;
using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Application.Interfaces;

public interface IAdminService
{
    Task<Result<UserDto>> CreateUser(CreateUserDto dto);
    Task<Result<List<UserDto>>> GetAllUsers();
    Task<Result<UserDto>> GetUserById(Guid id);
    Task<Result<UserEntity>> DeleteUser(Guid id, string? currentUserId);
    Task<Result<UserDto>> UpdateUser(UpdateUserDto dto);
    Task<Result<Dictionary<Guid, string>>> GetUsersRoles(List<Guid> ids);
    Task<Result<bool>> ResetTwoFactor(Guid userId);
    Result<int> GetCurrentRequisiteAssignmentAlgorithm();
    Task<Result<bool>> SetRequisiteAssignmentAlgorithm(int algorithm);
    Result<decimal> GetCurrentUsdtExchangeRate();
    Result<bool> SetUsdtExchangeRate(decimal rate);
}