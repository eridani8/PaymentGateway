using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Application.Services;

public class AdminService(
    IMapper mapper,
    UserManager<UserEntity> userManager,
    RoleManager<IdentityRole> roleManager,
    IValidator<CreateUserDto> createValidator,
    ILogger<AdminService> logger) : IAdminService
{
    public async Task<UserResponseDto> CreateUser(CreateUserDto dto)
    {
        var validation = await createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var existingUser = await userManager.FindByNameAsync(dto.Username);
        if (existingUser != null)
        {
            throw new DuplicatePaymentException();
        }

        var user = mapper.Map<UserEntity>(dto);

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            throw new ArgumentException();
        }
        
        foreach (var role in dto.Roles)
        {
            await userManager.AddToRoleAsync(user, role);
        }
        
        logger.LogInformation("Создание пользователя {username}", dto.Username);

        return mapper.Map<UserResponseDto>(user);
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsers()
    {
        var users = await userManager.Users.AsNoTracking().ToListAsync();
        return mapper.Map<List<UserResponseDto>>(users);
    }

    public async Task<UserResponseDto?> GetUserById(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        return mapper.Map<UserResponseDto>(user);
    }

    public async Task<bool> DeleteUser(Guid id, string? currentUserId)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return false;

        if (id.ToString() == currentUserId)
        {
            return false;
        }

        await userManager.DeleteAsync(user);
        
        logger.LogInformation("Удаление пользователя {username}", user.UserName);
        
        return true;
    }
}