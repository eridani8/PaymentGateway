using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Results;
using PaymentGateway.Core;
using PaymentGateway.Core.Entities;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Enums;
using PaymentGateway.Shared.Interfaces;

namespace PaymentGateway.Application.Services;

public class AdminService(
    IMapper mapper,
    UserManager<UserEntity> userManager,
    IValidator<CreateUserDto> createValidator,
    IValidator<UpdateUserDto> updateValidator,
    INotificationService notificationService,
    IOptions<GatewaySettings> gatewaySettings) : IAdminService
{
    public async Task<Result<UserDto>> CreateUser(CreateUserDto dto)
    {
        var validation = await createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            return Result.Failure<UserDto>(new ValidationError(validation.Errors.Select(e => e.ErrorMessage).ToList()));
        }
    
        var existingUser = await userManager.FindByNameAsync(dto.Username);
        if (existingUser != null)
        {
            return Result.Failure<UserDto>(Error.UserAlreadyExists);
        }
    
        var user = mapper.Map<UserEntity>(dto);
    
        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return Result.Failure<UserDto>(Error.UserCreationFailed(string.Join(", ", result.Errors.Select(e => e.Description))));
        }
        
        foreach (var role in dto.Roles)
        {
            await userManager.AddToRoleAsync(user, role);
        }
    
        var userDto = mapper.Map<UserDto>(user);
        await notificationService.NotifyUserUpdated(userDto);
        
        return Result.Success(userDto);
    }

    public async Task<Result<List<UserDto>>> GetAllUsers()
    {
        var users = await userManager.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
            
        return Result.Success(users.Select(mapper.Map<UserDto>).ToList());
    }

    public async Task<Result<UserDto>> GetUserById(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return Result.Failure<UserDto>(Error.UserNotFound);
        }
        return Result.Success(mapper.Map<UserDto>(user));
    }

    public async Task<Result<UserEntity>> DeleteUser(Guid id, string? currentUserId)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return Result.Failure<UserEntity>(Error.UserNotFound);
        }

        if (user.UserName == "root")
        {
            return Result.Failure<UserEntity>(Error.DeleteRootUserForbidden);
        }

        if (id.ToString() == currentUserId)
        {
            return Result.Failure<UserEntity>(Error.SelfDeleteForbidden);
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return Result.Failure<UserEntity>(Error.OperationFailed("удаление пользователя", 
                string.Join(", ", result.Errors.Select(e => e.Description))));
        }
        
        return Result.Success(user);
    }

    public async Task<Result<UserDto>> UpdateUser(UpdateUserDto dto)
    {
        var validation = await updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            return Result.Failure<UserDto>(new ValidationError(validation.Errors.Select(e => e.ErrorMessage).ToList()));
        }

        var user = await userManager.FindByIdAsync(dto.Id.ToString());
        if (user is null)
        {
            return Result.Failure<UserDto>(Error.UserNotFound);
        }

        if (user.UserName == "root")
        {
            return Result.Failure<UserDto>(Error.ModifyRootUserForbidden);
        }

        mapper.Map(dto, user);
        var result = await userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
        {
            return Result.Failure<UserDto>(Error.UserUpdateFailed(
                string.Join(", ", result.Errors.Select(e => e.Description))));
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        
        var removeRolesResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeRolesResult.Succeeded)
        {
            return Result.Failure<UserDto>(Error.OperationFailed("обновление ролей пользователя",
                string.Join(", ", removeRolesResult.Errors.Select(e => e.Description))));
        }
        
        var addRolesResult = await userManager.AddToRolesAsync(user, dto.Roles);
        if (!addRolesResult.Succeeded)
        {
            return Result.Failure<UserDto>(Error.OperationFailed("обновление ролей пользователя",
                string.Join(", ", addRolesResult.Errors.Select(e => e.Description))));
        }
        
        var userDto = mapper.Map<UserDto>(user);
        await notificationService.NotifyUserUpdated(userDto);
        
        return Result.Success(userDto);
    }

    public async Task<Result<Dictionary<Guid, string>>> GetUsersRoles(List<Guid> userIds)
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
            
        return Result.Success(roles);
    }
    
    public async Task<Result<bool>> ResetTwoFactorAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Result.Failure<bool>(Error.UserNotFound);
        }
        
        user.TwoFactorEnabled = false;
        user.TwoFactorSecretKey = null;
        
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return Result.Failure<bool>(Error.OperationFailed("сброс двухфакторной аутентификации",
                string.Join(", ", result.Errors.Select(e => e.Description))));
        }
        
        var userDto = mapper.Map<UserDto>(user);
        await notificationService.NotifyUserUpdated(userDto);
        return Result.Success(true);
    }

    public Result<int> GetCurrentRequisiteAssignmentAlgorithm()
    {
        return Result.Success((int)gatewaySettings.Value.AppointmentAlgorithm);
    }

    public Result<bool> SetRequisiteAssignmentAlgorithm(int algorithm)
    {
        if (!Enum.IsDefined(typeof(RequisiteAssignmentAlgorithm), algorithm))
        {
            return Result.Failure<bool>(
                Error.OperationFailed("изменение алгоритма назначения реквизитов", "Указан недопустимый алгоритм"));
        }
            
        gatewaySettings.Value.AppointmentAlgorithm = (RequisiteAssignmentAlgorithm)algorithm;
        return Result.Success(true);
    }
}