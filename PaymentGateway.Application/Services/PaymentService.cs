using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Results;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Interfaces;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.Enums;
using PaymentGateway.Shared.Interfaces;
using Npgsql;

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
    public async Task<Result<PaymentDto>> CreatePayment(PaymentCreateDto dto)
    {
        var validation = await createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            return Result.Failure<PaymentDto>(new ValidationError(validation.Errors.Select(e => e.ErrorMessage)));
        }

        var containsEntity = await unit.PaymentRepository.GetExistingPayment(dto.ExternalPaymentId);
        if (containsEntity is not null)
        {
            return Result.Failure<PaymentDto>(Error.DuplicatePayment);
        }

        var entity = mapper.Map<PaymentEntity>(dto);

        await unit.PaymentRepository.Add(entity);
        await unit.Commit();

        var paymentDto = mapper.Map<PaymentDto>(entity);

        await notificationService.NotifyPaymentUpdated(paymentDto);

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
            return Result.Failure<PaymentEntity>(Error.PaymentNotFound);
        }

        if (payment.Status == PaymentStatus.Confirmed)
        {
            return Result.Failure<PaymentEntity>(Error.PaymentAlreadyConfirmed);
        }

        if (payment.Requisite is not { } requisite)
        {
            return Result.Failure<PaymentEntity>(Error.RequisiteNotAttached);
        }

        var user = await userManager.FindByIdAsync(currentUserId.ToString());
        if (user is null)
        {
            return Result.Failure<PaymentEntity>(Error.InsufficientPermissions);
        }

        var userRoles = await userManager.GetRolesAsync(user);

        if (payment.Requisite.UserId != currentUserId && !userRoles.Contains("Admin") && !userRoles.Contains("Support"))
        {
            return Result.Failure<PaymentEntity>(Error.InsufficientPermissionsForPayment);
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
            return Result.Failure<PaymentEntity>(Error.PaymentNotFound);
        }

        var user = await userManager.FindByIdAsync(currentUserId.ToString());
        if (user is null)
        {
            return Result.Failure<PaymentEntity>(Error.InsufficientPermissions);
        }

        var userRoles = await userManager.GetRolesAsync(user);
        if (!userRoles.Contains("Admin") && !userRoles.Contains("Support"))
        {
            return Result.Failure<PaymentEntity>(Error.InsufficientPermissions);
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
            return Result.Failure<PaymentDto>(Error.PaymentNotFound);
        }
        
        return Result.Success(mapper.Map<PaymentDto>(entity));
    }

    public async Task<Result<PaymentDto>> DeletePayment(Guid id)
    {
        var entity = await unit.PaymentRepository.PaymentById(id);
        if (entity is null)
        {
            return Result.Failure<PaymentDto>(Error.PaymentNotFound);
        }

        try
        {
            if (entity.Requisite != null && entity.Requisite.PaymentId == id)
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
        catch (PostgresException ex) when (ex.SqlState == "23503")
        {
            logger.LogWarning("Невозможно удалить платеж {PaymentId}, так как он используется в других таблицах", id);
            return Result.Failure<PaymentDto>(Error.OperationFailed("Невозможно удалить платеж, так как он используется в других таблицах"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при удалении платежа {PaymentId}", id);
            return Result.Failure<PaymentDto>(Error.OperationFailed(ex.Message));
        }
    }
}