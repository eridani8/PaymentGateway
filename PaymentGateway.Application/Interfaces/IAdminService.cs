using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Application.Interfaces;

public interface IAdminService
{
    Task<UserDto?> CreateUser(CreateUserDto dto);
    Task<IEnumerable<UserDto>> GetAllUsers();
    Task<UserDto?> GetUserById(Guid id);
    Task<UserDto?> DeleteUser(Guid id, string? currentUserId);
    Task<UserDto?> UpdateUser(UpdateUserDto dto);
}