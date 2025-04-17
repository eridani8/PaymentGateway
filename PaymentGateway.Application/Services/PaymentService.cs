using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.Enums;
using PaymentGateway.Shared.Interfaces;

namespace PaymentGateway.Application.Services;

public class PaymentService(
    IUnitOfWork unit,
    IMapper mapper,
    IValidator<PaymentCreateDto> createValidator,
    IValidator<PaymentManualConfirmDto> manualConfirmValidator,
    IValidator<PaymentCancelDto> cancelValidator,
    ILogger<PaymentService> logger,
    UserManager<UserEntity> userManager,
    INotificationService notificationService,
    IPaymentConfirmationService paymentConfirmationService) : IPaymentService
{
    public async Task<PaymentDto?> CreatePayment(PaymentCreateDto dto)
    {
        var validation = await createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var containsEntity = await unit.PaymentRepository
            .QueryableGetAll()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ExternalPaymentId == dto.ExternalPaymentId);
        if (containsEntity is not null)
        {
            throw new DuplicatePaymentException();
        }

        var entity = mapper.Map<PaymentEntity>(dto);

        await unit.PaymentRepository.Add(entity);
        await unit.Commit();

        var paymentDto = mapper.Map<PaymentDto>(entity);

        logger.LogInformation("Создание платежа {paymentId} на сумму {amount}", entity.Id, entity.Amount);

        await notificationService.NotifyPaymentUpdated(paymentDto);

        return paymentDto;
    }

    public async Task<PaymentDto?> ManualConfirmPayment(PaymentManualConfirmDto dto, Guid currentUserId)
    {
        var validation = await manualConfirmValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var payment = await unit.PaymentRepository.GetById(dto.PaymentId,
            p => p.Requisite,
            p => p.Transaction);

        if (payment is null)
        {
            throw new PaymentNotFound();
        }

        if (payment.Status == PaymentStatus.Confirmed)
        {
            throw new ManualConfirmException("Платеж уже подтвержден");
        }

        if (payment.Requisite is not { } requisite)
        {
            throw new ManualConfirmException("К платежу не привязан реквизит");
        }

        var user = await userManager.FindByIdAsync(currentUserId.ToString());
        if (user is null)
        {
            throw new ManualConfirmException("Недостаточно прав");
        }

        var userRoles = await userManager.GetRolesAsync(user);

        if (payment.Requisite.UserId != currentUserId ||
            !(userRoles.Contains("User") && userRoles.Contains("Admin") && userRoles.Contains("Support")) ||
            !userRoles.Contains("Support"))
        {
            throw new ManualConfirmException("Недостаточно прав для подтверждения платежа");
        }

        payment.ManualConfirm(user.Id);

        await paymentConfirmationService.ProcessPaymentConfirmation(payment, requisite, payment.Amount);

        logger.LogInformation("Ручное подтверждение платежа {paymentId}", payment.Id);

        return mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto?> CancelPayment(PaymentCancelDto dto, Guid currentUserId)
    {
        var validation = await cancelValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }
        
        var payment = await unit.PaymentRepository.GetById(dto.PaymentId,
            p => p.Requisite,
            p => p.Transaction);

        if (payment is null)
        {
            throw new PaymentNotFound();
        }

        var user = await userManager.FindByIdAsync(currentUserId.ToString());
        if (user is null)
        {
            throw new ManualConfirmException("Недостаточно прав");
        }

        var userRoles = await userManager.GetRolesAsync(user);
        if (!userRoles.Contains("Admin") && !userRoles.Contains("Support"))
        {
            throw new ManualConfirmException("Недостаточно прав для отмены платежа");
        }

        payment.Status = PaymentStatus.Canceled;
        payment.ExpiresAt = null;

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
        
        logger.LogInformation("Отмена платежа {paymentId} пользователем {userId}", payment.Id, currentUserId);
        await notificationService.NotifyPaymentUpdated(paymentDto);

        return paymentDto;
    }

    public async Task<IEnumerable<PaymentDto>> GetAllPayments()
    {
        var entities = await unit.PaymentRepository.QueryableGetAll()
            .Include(p => p.Requisite)
            .Include(p => p.Transaction)
            .AsNoTracking()
            .ToListAsync();
        return mapper.Map<IEnumerable<PaymentDto>>(entities);
    }

    public async Task<IEnumerable<PaymentDto>> GetUserPayments(Guid userId)
    {
        var entities = await unit.PaymentRepository.QueryableGetAll()
            .Include(p => p.Requisite)
            .Include(p => p.Transaction)
            .Where(p => p.UserId == userId || (p.Requisite != null && p.Requisite.UserId == userId))
            .AsNoTracking()
            .ToListAsync();
        return mapper.Map<IEnumerable<PaymentDto>>(entities);
    }

    public async Task<PaymentDto?> GetPaymentById(Guid id)
    {
        var entity = await unit.PaymentRepository.GetById(id,
            p => p.Requisite,
            p => p.Transaction,
            p => p.ManualConfirmUser,
            p => p.CanceledByUser);
        return entity is not null ? mapper.Map<PaymentDto>(entity) : null;
    }

    public async Task<PaymentDto?> DeletePayment(Guid id)
    {
        var entity = await unit.PaymentRepository.GetById(id, 
            p => p.Requisite, 
            p => p.Transaction,
            p => p.ManualConfirmUser,
            p => p.CanceledByUser);
        if (entity is null) return null;

        if (entity.Requisite != null && entity.Requisite.PaymentId == id)
        {
            entity.Requisite.Status = RequisiteStatus.Active;
            entity.Requisite.PaymentId = null;
            entity.Requisite.Payment = null;
        }

        unit.PaymentRepository.Delete(entity);
        await unit.Commit();

        var paymentDto = mapper.Map<PaymentDto>(entity);

        logger.LogInformation("Удаление платежа {paymentId}", entity.Id);
        await notificationService.NotifyPaymentUpdated(paymentDto);

        return paymentDto;
    }
}