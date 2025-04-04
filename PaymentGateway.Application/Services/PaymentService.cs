using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Application.DTOs.Payment;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Application.Services;

public class PaymentService(IUnitOfWork unit, IMapper mapper, IValidator<PaymentCreateDto> paymentCreateValidator) : IPaymentService
{
    public async Task<PaymentResponseDto> CreatePayment(PaymentCreateDto dto)
    {
        var validationResult = await paymentCreateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
        }
        
        var payment = new PaymentEntity(dto.PaymentId, dto.Amount, dto.UserId);

        await unit.PaymentRepository.Add(payment);
        await unit.Commit();
        
        return mapper.Map<PaymentResponseDto>(payment);
    }

    public async Task<IEnumerable<PaymentResponseDto>> GetAllPayments()
    {
        var entities = await unit.PaymentRepository.GetAll().AsNoTracking().ToListAsync();
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

        return true;
    }
}