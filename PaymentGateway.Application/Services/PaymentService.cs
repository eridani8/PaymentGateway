using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared.DTOs.Payment;

namespace PaymentGateway.Application.Services;

public class PaymentService(
    IUnitOfWork unit,
    IMapper mapper,
    IValidator<PaymentCreateDto> validator,
    ILogger<PaymentService> logger) : IPaymentService
{
    public async Task<PaymentDto?> CreatePayment(PaymentCreateDto dto)
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

        return mapper.Map<PaymentDto>(entity);
    }

    public async Task<IEnumerable<PaymentDto>> GetAllPayments()
    {
        var entities = await unit.PaymentRepository.QueryableGetAll().AsNoTracking().ToListAsync();
        return mapper.Map<IEnumerable<PaymentDto>>(entities);
    }

    public async Task<PaymentDto?> GetPaymentById(Guid id)
    {
        var entity = await unit.PaymentRepository.GetById(id);
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