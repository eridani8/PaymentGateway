﻿using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Web.Interfaces;

public interface IAdminService
{
    Task<List<UserDto>> GetUsers();
    Task<UserDto?> CreateUser(CreateUserDto dto);
    Task<bool> DeleteUser(Guid id);
    Task<bool> UpdateUser(UpdateUserDto dto);
}