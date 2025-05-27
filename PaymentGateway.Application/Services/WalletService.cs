using System.Security.Claims;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Extensions;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Results;
using PaymentGateway.Core.Entities;
using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Application.Services;

public class WalletService(
    IValidator<DepositDto> validator,
    UserManager<UserEntity> userManager,
    INotificationService notificationService,
    IMapper mapper,
    ILogger<WalletService> logger) : IWalletService
{
    public async Task<Result> Deposit(DepositDto dto, ClaimsPrincipal currentUser)
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            return Result.Failure(new ValidationError(validation.Errors.Select(e => e.ErrorMessage)));
        }
        
        var user = await userManager.FindByIdAsync(dto.UserId.ToString());
        if (user is not { IsActive: true })
        {
            return Result.Failure(UserErrors.UserNotFound);
        }
        
        var oldBalance = user.Balance;
        
        user.Balance += dto.Amount;
        await userManager.UpdateAsync(user);
        
        logger.LogInformation("Пополнение счета пользователя {UserId} на {DepositAmount} [{CurrentUser}]. Было {OldBalance}, стало {NewBalance}", dto.UserId, dto.Amount, currentUser.GetCurrentUsername(), oldBalance, user.Balance);

        var userDto = mapper.Map<UserDto>(user);
        var walletDto = mapper.Map<WalletDto>(user);
        
        await notificationService.NotifyUserUpdated(userDto);
        await notificationService.NotifyWalletUpdated(walletDto);
        
        return Result.Success();
    }
}