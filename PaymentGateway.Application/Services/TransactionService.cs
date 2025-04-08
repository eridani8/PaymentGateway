using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.DTOs.Transaction;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Enums;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Application.Services;

public class TransactionService(
    IUnitOfWork unit,
    IMapper mapper,
    IValidator<TransactionCreateDto> validator,
    ILogger<TransactionService> logger)
    : ITransactionService
{
    public async Task<TransactionResponseDto> CreateTransaction(TransactionCreateDto dto)
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var requisite = await unit.RequisiteRepository
            .QueryableGetAll()
            .Include(r => r.Payment)
            .FirstOrDefaultAsync(r => r.PaymentData == dto.PaymentData);

        if (requisite?.Payment is not { } payment)
        {
            throw new RequisiteNotFound();
        }

        if (payment.Amount != dto.ExtractedAmount)
        {
            throw new WrongPaymentAmount($"Сумма платежа {dto.ExtractedAmount}, ожидалось {payment.Amount}");
        }

        var entity = new TransactionEntity
        {
            Id = Guid.NewGuid(),
            ExtractedAmount = dto.ExtractedAmount,
            Source = dto.Source,
            ReceivedAt = dto.ReceivedAt,
            PaymentId = requisite.Payment.Id,
            Payment = requisite.Payment,
            RequisiteId = requisite.Id,
            Requisite = requisite,
            RawMessage = dto.RawMessage
        };
        
        logger.LogInformation("Поступление платежа на сумму {amount}", entity.ExtractedAmount);
        
        payment.ConfirmTransaction(entity.Id);
        requisite.ReleaseAfterPayment(dto.ExtractedAmount);

        logger.LogInformation("Освобождение реквизита {requisiteId}", requisite.Id);

        await unit.TransactionRepository.Add(entity);
        await unit.Commit();

        return mapper.Map<TransactionResponseDto>(entity);
    }
}