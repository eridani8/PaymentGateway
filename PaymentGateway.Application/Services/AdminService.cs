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
    IValidator<CreateUserDto> createValidator,
    ILogger<AdminService> logger) : IAdminService
{
    public async Task<UserDto> CreateUser(CreateUserDto dto)
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
            throw new CreateUserException(result.Errors.GetErrors());
        }
        
        foreach (var role in dto.Roles)
        {
            await userManager.AddToRoleAsync(user, role);
        }
        
        logger.LogInformation("Создание пользователя {username}", dto.Username);
    
        return mapper.Map<UserDto>(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsers()
    {
        var users = await userManager.Users.AsNoTracking().ToListAsync();
        var userDtos = new List<UserDto>();
        
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            var userDto = mapper.Map<UserDto>(user);
            userDto.Roles.AddRange(roles);
            userDtos.Add(userDto);
        }
        
        return userDtos;
    }

    public async Task<UserDto?> GetUserById(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return null;
            
        var roles = await userManager.GetRolesAsync(user);
        var userDto = mapper.Map<UserDto>(user);
        userDto.Roles.AddRange(roles);
        
        return userDto;
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