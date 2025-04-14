using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Web.Interfaces;

public interface IAdminService
{
    Task<List<UserDto>> GetUsers();
    Task<UserDto?> CreateUser(CreateUserDto dto);
    Task<UserDto?> DeleteUser(Guid id);
    Task<UserDto?> UpdateUser(UpdateUserDto dto);
}