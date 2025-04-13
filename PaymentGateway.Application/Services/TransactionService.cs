using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.DTOs.Transaction;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Application.Services;

public class TransactionService(
    IUnitOfWork unit,
    IMapper mapper,
    IValidator<TransactionCreateDto> validator,
    ILogger<TransactionService> logger)
    : ITransactionService
{
    public async Task<TransactionDto> CreateTransaction(TransactionCreateDto dto)
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

        var transaction = mapper.Map<TransactionEntity>(dto);
        
        logger.LogInformation("Поступление платежа на сумму {amount}", transaction.ExtractedAmount);
        
        payment.ConfirmTransaction(transaction);
        requisite.ReleaseAfterPayment(dto.ExtractedAmount, out var status);
        logger.LogInformation("Освобождение реквизита {requisiteId}", requisite.Id);
        if (status == RequisiteStatus.Cooldown)
        {
            logger.LogInformation("Статус реквизита {status} {requisiteId} на {sec} сек.", status, requisite.Id, (int)requisite.Cooldown.TotalSeconds);   
        }

        await unit.TransactionRepository.Add(transaction);
        await unit.Commit();

        return mapper.Map<TransactionDto>(transaction);
    }
}