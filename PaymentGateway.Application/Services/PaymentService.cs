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
    ILogger<PaymentService> logger,
    UserManager<UserEntity> userManager,
    INotificationService notificationService) : IPaymentService
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
        
        logger.LogInformation("Создание платежа {paymentId} на сумму {amount}", entity.Id, entity.Amount);

        return mapper.Map<PaymentDto>(entity);
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
            return null;
        }
        
        if (paymentEntity.Requisite.UserId != currentUserId) return null;
        
        logger.LogInformation("Ручное подтверждение платежа {paymentId}", paymentEntity.Id);
        
        paymentEntity.ManualConfirm();
        requisite.ReleaseAfterPayment(paymentEntity.Amount, out var status);
        logger.LogInformation("Освобождение реквизита {requisiteId}", requisite.Id);
        if (status == RequisiteStatus.Cooldown)
        {
            logger.LogInformation("Статус реквизита {status} {requisiteId} на {sec} сек.", status, requisite.Id, (int)requisite.Cooldown.TotalSeconds);   
        }
        
        requisite.User.ReceivedDailyFunds += paymentEntity.Amount;
        logger.LogInformation("Увеличение суточных поступлений пользователя {userId} на {amount}, текущая сумма: {total}", requisite.User.Id, paymentEntity.Amount, requisite.User.ReceivedDailyFunds);
        await userManager.UpdateAsync(requisite.User);
        
        unit.RequisiteRepository.Update(requisite);
        await unit.Commit();

        var payment = mapper.Map<PaymentDto>(paymentEntity);
        
        await notificationService.NotifyRequisiteUpdated(mapper.Map<RequisiteDto>(requisite));
        await notificationService.NotifyPaymentUpdated(payment);
        
        return payment;
    }

    public async Task<IEnumerable<PaymentDto>> GetAllPayments()
    {
        var entities = await unit.PaymentRepository.QueryableGetAll().AsNoTracking().ToListAsync();
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
        
        logger.LogInformation("Удаление платежа {paymentId}", entity.Id);

        return mapper.Map<PaymentDto>(entity);
    }
}