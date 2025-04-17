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
using PaymentGateway.Shared.Interfaces;

namespace PaymentGateway.Application.Services;

public class PaymentService(
    IUnitOfWork unit,
    IMapper mapper,
    IValidator<PaymentCreateDto> createValidator,
    IValidator<PaymentManualConfirmDto> manualConfirmValidator,
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
        
        var paymentEntity = await unit.PaymentRepository
            .QueryableGetAll()
            .Include(p => p.Requisite)
            .FirstOrDefaultAsync(p => p.Id == dto.PaymentId);

        if (paymentEntity is null)
        {
            throw new PaymentNotFound();
        }

        if (paymentEntity.Requisite is not { } requisite)
        {
            throw new ManualConfirmException();
        }

        var user = await userManager.FindByIdAsync(currentUserId.ToString());
        if (user is null)
        {
            throw new ManualConfirmException("Недостаточно прав");
        }
        
        var userRoles = await userManager.GetRolesAsync(user);

        if (paymentEntity.Requisite.UserId != currentUserId || 
            !(userRoles.Contains("User") && userRoles.Contains("Admin") && userRoles.Contains("Support")) || 
            !userRoles.Contains("Support"))
        {
            throw new ManualConfirmException("Подтвердить платеж может только владелец или служба поддержки");
        }
        
        logger.LogInformation("Ручное подтверждение платежа {paymentId}", paymentEntity.Id);
        
        paymentEntity.ManualConfirm();
        
        await paymentConfirmationService.ProcessPaymentConfirmation(paymentEntity, requisite, paymentEntity.Amount);

        var payment = mapper.Map<PaymentDto>(paymentEntity);
        
        return payment;
    }

    public async Task<IEnumerable<PaymentDto>> GetAllPayments()
    {
        var entities = await unit.PaymentRepository.QueryableGetAll()
            .Include(p => p.Requisite)
            .AsNoTracking()
            .ToListAsync();
        return mapper.Map<IEnumerable<PaymentDto>>(entities);
    }

    public async Task<IEnumerable<PaymentDto>> GetUserPayments(Guid userId)
    {
        var entities = await unit.PaymentRepository.QueryableGetAll()
            .Include(p => p.Requisite)
            .Where(p => p.Requisite != null && p.Requisite.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
        
        return mapper.Map<IEnumerable<PaymentDto>>(entities);
    }

    public async Task<PaymentDto?> GetPaymentById(Guid id)
    {
        var entity = await unit.PaymentRepository.GetById(id, 
            p => p.Requisite, 
            p => p.Transaction);
        return entity is not null ? mapper.Map<PaymentDto>(entity) : null;
    }

    public async Task<PaymentDto?> DeletePayment(Guid id)
    {
        var entity = await unit.PaymentRepository.GetById(id);
        if (entity is null) return null;

        unit.PaymentRepository.Delete(entity);
        await unit.Commit();

        var paymentDto = mapper.Map<PaymentDto>(entity);
        
        logger.LogInformation("Удаление платежа {paymentId}", entity.Id);
        await notificationService.NotifyPaymentUpdated(paymentDto);

        return paymentDto;
    }
}