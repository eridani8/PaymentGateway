using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Application.Interfaces;

public interface IAdminService
{
    Task<UserResponseDto> CreateUser(CreateUserDto dto);
    Task<IEnumerable<UserResponseDto>> GetAllUsers();
    Task<UserResponseDto?> GetUserById(Guid id);
    Task<bool> DeleteUser(Guid id, string? currentUserId);
}