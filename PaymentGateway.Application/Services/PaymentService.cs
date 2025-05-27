using System.Security.Claims;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Results;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Interfaces;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.Enums;
using Npgsql;
using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Application.Services;

public class PaymentService(
    IUnitOfWork unit,
    IMapper mapper,
    IValidator<PaymentCreateDto> createValidator,
    IValidator<PaymentManualConfirmDto> manualConfirmValidator,
    IValidator<PaymentCancelDto> cancelValidator,
    UserManager<UserEntity> userManager,
    INotificationService notificationService,
    IPaymentConfirmationService paymentConfirmationService,
    ILogger<PaymentService> logger) : IPaymentService
{
    public async Task<Result<PaymentDto>> CreatePayment(PaymentCreateDto dto, ClaimsPrincipal userClaim)
    {
        var validation = await createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            return Result.Failure<PaymentDto>(new ValidationError(validation.Errors.Select(e => e.ErrorMessage)));
        }

        var user = await userManager.GetUserAsync(userClaim);
        if (user is null)
        {
            return Result.Failure<PaymentDto>(UserErrors.UserNotFound);
        }

        if (user.Balance < dto.Amount)
        {
            return Result.Failure<PaymentDto>(PaymentErrors.NotEnoughFunds);
        }

        var containsEntity = await unit.PaymentRepository.GetExistingPayment(dto.ExternalPaymentId);
        if (containsEntity is not null)
        {
            return Result.Failure<PaymentDto>(PaymentErrors.DuplicatePayment);
        }

        var entity = mapper.Map<PaymentEntity>(dto);

        await unit.PaymentRepository.Add(entity);
        await unit.Commit();
        
        var oldBalance = user.Balance;
        var oldFrozen = user.Frozen;
        
        user.Balance -= dto.Amount;
        user.Frozen += dto.Amount;
        
        await userManager.UpdateAsync(user);
        
        logger.LogInformation("Заморожено {Frozen} на счету пользователя {UserId}. Платеж {PaymentId}. Было на балансе {OldBalance}, стало {NewBalance}. Было заморожено {OldFrozen}, стало {NewFrozen}",
            dto.Amount, user.Id, entity.Id, oldBalance, user.Balance, oldFrozen, user.Frozen);

        var paymentDto = mapper.Map<PaymentDto>(entity);
        var userDto = mapper.Map<UserDto>(user);
        var walletDto = mapper.Map<WalletDto>(user);
        
        await notificationService.NotifyPaymentUpdated(paymentDto);
        await notificationService.NotifyUserUpdated(userDto);
        await notificationService.NotifyWalletUpdated(walletDto);

        return Result.Success(paymentDto);
    }

    public async Task<Result<PaymentEntity>> ManualConfirmPayment(PaymentManualConfirmDto dto, Guid currentUserId)
    {
        var validation = await manualConfirmValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            return Result.Failure<PaymentEntity>(new ValidationError(validation.Errors.Select(e => e.ErrorMessage)));
        }

        var payment = await unit.PaymentRepository.PaymentById(dto.PaymentId);
        if (payment is null)
        {
            return Result.Failure<PaymentEntity>(PaymentErrors.PaymentNotFound);
        }

        if (payment.Status == PaymentStatus.Confirmed)
        {
            return Result.Failure<PaymentEntity>(PaymentErrors.PaymentAlreadyConfirmed);
        }

        if (payment.Requisite is not { } requisite)
        {
            return Result.Failure<PaymentEntity>(PaymentErrors.RequisiteNotAttached);
        }

        var user = await userManager.FindByIdAsync(currentUserId.ToString());
        if (user is null)
        {
            return Result.Failure<PaymentEntity>(UserErrors.InsufficientPermissions);
        }

        var userRoles = await userManager.GetRolesAsync(user);

        if (payment.Requisite.UserId != currentUserId && !userRoles.Contains("Admin") && !userRoles.Contains("Support"))
        {
            return Result.Failure<PaymentEntity>(PaymentErrors.InsufficientPermissionsForPayment);
        }

        payment.ManualConfirm(user.Id);

        await paymentConfirmationService.ProcessPaymentConfirmation(payment, requisite, payment.Amount);
        
        var paymentDto = mapper.Map<PaymentDto>(payment);
        await notificationService.NotifyPaymentUpdated(paymentDto);
        var requisiteDto = mapper.Map<RequisiteDto>(requisite);
        await notificationService.NotifyRequisiteUpdated(requisiteDto);

        return Result.Success(payment);
    }

    public async Task<Result<PaymentEntity>> CancelPayment(PaymentCancelDto dto, Guid currentUserId)
    {
        var validation = await cancelValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            return Result.Failure<PaymentEntity>(new ValidationError(validation.Errors.Select(e => e.ErrorMessage)));
        }

        var payment = await unit.PaymentRepository.PaymentById(dto.PaymentId);
        if (payment is null)
        {
            return Result.Failure<PaymentEntity>(PaymentErrors.PaymentNotFound);
        }

        var user = await userManager.FindByIdAsync(currentUserId.ToString());
        if (user is null)
        {
            return Result.Failure<PaymentEntity>(UserErrors.InsufficientPermissions);
        }

        var userRoles = await userManager.GetRolesAsync(user);
        if (!userRoles.Contains("Admin") && !userRoles.Contains("Support"))
        {
            return Result.Failure<PaymentEntity>(UserErrors.InsufficientPermissions);
        }

        payment.Status = PaymentStatus.Canceled;
        payment.ExpiresAt = null;
        payment.CanceledByUserId = currentUserId;

        if (payment.Requisite is { } requisite)
        {
            requisite.Status = RequisiteStatus.Active;
            requisite.PaymentId = null;
            requisite.Payment = null;
            unit.RequisiteRepository.Update(requisite);
            await notificationService.NotifyRequisiteUpdated(mapper.Map<RequisiteDto>(requisite));
        }

        unit.PaymentRepository.Update(payment);

        await unit.Commit();

        var paymentDto = mapper.Map<PaymentDto>(payment);

        await notificationService.NotifyPaymentUpdated(paymentDto);

        return Result.Success(payment);
    }

    public async Task<Result<IEnumerable<PaymentDto>>> GetAllPayments()
    {
        var entities = await unit.PaymentRepository.GetAllPayments();
        return Result.Success(mapper.Map<IEnumerable<PaymentDto>>(entities));
    }

    public async Task<Result<IEnumerable<PaymentDto>>> GetUserPayments(Guid userId)
    {
        var entities = await unit.PaymentRepository.GetUserPayments(userId);
        return Result.Success(mapper.Map<IEnumerable<PaymentDto>>(entities));
    }

    public async Task<Result<PaymentDto>> GetPaymentById(Guid id)
    {
        var entity = await unit.PaymentRepository.PaymentById(id);
        if (entity is null)
        {
            return Result.Failure<PaymentDto>(PaymentErrors.PaymentNotFound);
        }
        
        return Result.Success(mapper.Map<PaymentDto>(entity));
    }

    public async Task<Result<PaymentDto>> DeletePayment(Guid id)
    {
        var entity = await unit.PaymentRepository.PaymentById(id);
        if (entity is null)
        {
            return Result.Failure<PaymentDto>(PaymentErrors.PaymentNotFound);
        }

        try
        {
            if (entity.Requisite is not null && entity.Requisite.PaymentId == id)
            {
                entity.Requisite.Status = RequisiteStatus.Active;
                entity.Requisite.PaymentId = null;
                entity.Requisite.Payment = null;
                unit.RequisiteRepository.Update(entity.Requisite);
            }

            unit.PaymentRepository.Delete(entity);
            await unit.Commit();

            var paymentDto = mapper.Map<PaymentDto>(entity);
            await notificationService.NotifyPaymentDeleted(id, entity.UserId);

            return Result.Success(paymentDto);
        }
        catch (DbUpdateException e) when(e.InnerException is PostgresException { SqlState: "23503" })
        {
            return Result.Failure<PaymentDto>(Error.OperationFailed("Невозможно удалить платеж, так как он используется в других таблицах"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при удалении платежа {PaymentId}", id);
            return Result.Failure<PaymentDto>(Error.OperationFailed(ex.Message));
        }
    }
}