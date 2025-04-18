using PaymentGateway.Core.Entities;
using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Application.Interfaces;

public interface IAdminService
{
    Task<UserDto?> CreateUser(CreateUserDto dto);
    Task<List<UserDto>> GetAllUsers();
    Task<UserDto?> GetUserById(Guid id);
    Task<UserEntity?> DeleteUser(Guid id, string? currentUserId);
    Task<UserDto?> UpdateUser(UpdateUserDto dto);
}