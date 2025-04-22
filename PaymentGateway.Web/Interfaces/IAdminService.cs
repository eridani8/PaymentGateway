using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Web.Interfaces;

public interface IAdminService
{
    Task<List<UserDto>> GetAllUsers();
    Task<UserDto?> CreateUser(CreateUserDto dto);
    Task<bool> DeleteUser(Guid id);
    Task<UserDto?> UpdateUser(UpdateUserDto dto);
    Task<UserDto?> GetUserById(Guid userId);
    Task<Dictionary<Guid, string>> GetUsersRoles(List<Guid> users);
    Task<bool> ResetTwoFactorAsync(Guid userId);
}