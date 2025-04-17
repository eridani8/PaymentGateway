using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared.DTOs.Transaction;
using PaymentGateway.Shared.Interfaces;

namespace PaymentGateway.Application.Services;

public class TransactionService(
    IUnitOfWork unit,
    IMapper mapper,
    IValidator<TransactionCreateDto> validator,
    ILogger<TransactionService> logger,
    IPaymentConfirmationService paymentConfirmationService)
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
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.PaymentData == dto.PaymentData);

        if (requisite?.Payment is not { } payment)
        {
            throw new RequisiteNotFound("Реквизит не найден или не связан с платежом");
        }

        if (payment.Amount != dto.ExtractedAmount)
        {
            throw new WrongPaymentAmount($"Сумма платежа {dto.ExtractedAmount}, ожидалось {payment.Amount}");
        }

        var transaction = mapper.Map<TransactionEntity>(dto);
        
        logger.LogInformation("Поступление платежа на сумму {amount}", transaction.ExtractedAmount);
        
        payment.ConfirmTransaction(transaction);

        await unit.TransactionRepository.Add(transaction);
        await paymentConfirmationService.ProcessPaymentConfirmation(payment, requisite, dto.ExtractedAmount);
        await unit.Commit();

        return mapper.Map<TransactionDto>(transaction);
    }
}