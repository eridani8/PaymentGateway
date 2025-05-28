using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Enums;
using PaymentGateway.Shared.Types;
using PaymentGateway.Web.Services;

namespace PaymentGateway.Web.Interfaces;

public interface IAdminService
{
    Task<List<UserDto>> GetAllUsers();
    Task<Response> CreateUser(CreateUserDto dto);
    Task<Response> DeleteUser(Guid id);
    Task<Response> UpdateUser(UpdateUserDto dto);
    Task<UserDto?> GetUserById(Guid userId);
    Task<Dictionary<Guid, string>> GetUsersRoles(List<Guid> users);
    Task<Response> ResetTwoFactor(Guid userId);
    Task<RequisiteAssignmentAlgorithm> GetCurrentRequisiteAssignmentAlgorithm();
    Task<Response> SetRequisiteAssignmentAlgorithm(int algorithm);
    Task<decimal> GetCurrentUsdtExchangeRate();
    Task<Response> SetCurrentUsdtExchangeRate(decimal rate);
}