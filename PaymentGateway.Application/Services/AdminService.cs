using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Interfaces;

namespace PaymentGateway.Application.Services;

public class AdminService(
    IMapper mapper,
    UserManager<UserEntity> userManager,
    IValidator<CreateUserDto> createValidator,
    IValidator<UpdateUserDto> updateValidator,
    ILogger<AdminService> logger,
    INotificationService notificationService) : IAdminService
{
    public async Task<UserDto?> CreateUser(CreateUserDto dto)
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
    
        var roles = await userManager.GetRolesAsync(user);
        var userDto = mapper.Map<UserDto>(user);
        userDto.Roles.AddRange(roles);
        
        await notificationService.NotifyUserUpdated(userDto);
        
        return userDto;
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

    public async Task<UserDto?> DeleteUser(Guid id, string? currentUserId)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return null;

        if (user.UserName == "root")
        {
            return null;
        }

        if (id.ToString() == currentUserId)
        {
            return null;
        }

        await userManager.DeleteAsync(user);
        
        logger.LogInformation("Удаление пользователя {username}", user.UserName);
        
        return mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> UpdateUser(UpdateUserDto dto)
    {
        var validation = await updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var user = await userManager.FindByIdAsync(dto.Id.ToString());
        if (user is null)
        {
            return null;
        }

        if (user.UserName == "root")
        {
            throw new InvalidOperationException("Root user cannot be modified");
        }

        mapper.Map(dto, user);
        var result = await userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
        {
            return null;
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        await userManager.RemoveFromRolesAsync(user, currentRoles);
        await userManager.AddToRolesAsync(user, dto.Roles);
        
        logger.LogInformation("Обновление пользователя {username}", user.UserName);
        
        var updatedRoles = await userManager.GetRolesAsync(user);
        var userDto = mapper.Map<UserDto>(user);
        userDto.Roles.AddRange(updatedRoles);
        
        await notificationService.NotifyUserUpdated(userDto);
        
        return userDto;
    }
}