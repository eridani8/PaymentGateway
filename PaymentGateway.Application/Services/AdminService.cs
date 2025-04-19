using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    
        var userDto = mapper.Map<UserDto>(user);
        await notificationService.NotifyUserUpdated(userDto);
        
        return userDto;
    }

    public async Task<List<UserDto>> GetAllUsers()
    {
        var users = await userManager.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
        
        return users.Select(user => mapper.Map<UserDto>(user)).ToList();
    }

    public async Task<UserDto?> GetUserById(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        return user == null ? null : mapper.Map<UserDto>(user);
    }

    public async Task<UserEntity?> DeleteUser(Guid id, string? currentUserId)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return null;

        if (user.UserName == "root")
        {
            throw new DeleteUserException("Нельзя удалить root пользователя");
        }

        if (id.ToString() == currentUserId)
        {
            throw new DeleteUserException("Нельзя удалить себя");
        }

        await userManager.DeleteAsync(user);
        
        return user;
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
        
        var userDto = mapper.Map<UserDto>(user);
        await notificationService.NotifyUserUpdated(userDto);
        
        return userDto;
    }

    public async Task<Dictionary<Guid, string>> GetUsersRoles(List<Guid> userIds)
    {
        var users = await userManager.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();
        
        var roles = new Dictionary<Guid, string>();
        
        foreach (var user in users)
        {
            var userRoles = await userManager.GetRolesAsync(user);
            roles[user.Id] = string.Join(",", userRoles);
        }
        
        return roles;
    }
}