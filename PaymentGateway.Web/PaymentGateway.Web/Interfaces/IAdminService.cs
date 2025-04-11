using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Web.Interfaces;

public interface IAdminService
{
    Task<List<UserResponseDto>> GetUsers();
}