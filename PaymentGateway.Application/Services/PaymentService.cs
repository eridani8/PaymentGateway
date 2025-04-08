using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentGateway.Application.DTOs.Payment;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Enums;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Application.Services;

public class PaymentService(
    IUnitOfWork unit,
    IMapper mapper,
    IValidator<PaymentCreateDto> validator,
    ILogger<PaymentService> logger) : IPaymentService
{
    public async Task<PaymentResponseDto> CreatePayment(PaymentCreateDto dto)
    {
        var validation = await validator.ValidateAsync(dto);
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

        return mapper.Map<PaymentResponseDto>(entity);
    }

    public async Task<IEnumerable<PaymentResponseDto>> GetAllPayments()
    {
        var entities = await unit.PaymentRepository.QueryableGetAll().AsNoTracking().ToListAsync();
        return mapper.Map<IEnumerable<PaymentResponseDto>>(entities);
    }

    public async Task<PaymentResponseDto?> GetPaymentById(Guid id)
    {
        var entity = await unit.PaymentRepository.GetById(id);
        return entity is not null ? mapper.Map<PaymentResponseDto>(entity) : null;
    }

    public async Task<bool> DeletePayment(Guid id)
    {
        var entity = await unit.PaymentRepository.GetById(id);
        if (entity is null) return false;

        unit.PaymentRepository.Delete(entity);
        await unit.Commit();
        
        logger.LogInformation("Удаление платежа {paymentId}", entity.Id);

        return true;
    }
}