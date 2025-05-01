using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Infrastructure.Interfaces;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.Transaction;
using PaymentGateway.Shared.Enums;
using PaymentGateway.Shared.Interfaces;

namespace PaymentGateway.Application.Services;

public class TransactionService(
    IUnitOfWork unit,
    IMapper mapper,
    IValidator<TransactionCreateDto> validator,
    ILogger<TransactionService> logger,
    IPaymentConfirmationService paymentConfirmationService,
    INotificationService notificationService)
    : ITransactionService
{
    public async Task<TransactionDto?> CreateTransaction(TransactionCreateDto dto)
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        try
        {
            var requisite = await unit.RequisiteRepository.GetRequisiteByPaymentData(dto.PaymentData);

            if (requisite?.Payment is not { } payment)
            {
                throw new RequisiteNotFound("Реквизит не найден или не связан с платежом");
            }

            if (payment.Amount != dto.ExtractedAmount)
            {
                throw new WrongPaymentAmount($"Сумма платежа {dto.ExtractedAmount}, ожидалось {payment.Amount}");
            }

            var transactionEntity = mapper.Map<TransactionEntity>(dto);

            logger.LogInformation("Поступление платежа на сумму {amount}", transactionEntity.ExtractedAmount);

            payment.ConfirmTransaction(transactionEntity);
            await unit.TransactionRepository.Add(transactionEntity);
            await paymentConfirmationService.ProcessPaymentConfirmation(payment, requisite, dto.ExtractedAmount);

            await unit.Commit();

            var paymentDto = mapper.Map<PaymentDto>(payment);
            await notificationService.NotifyPaymentUpdated(paymentDto);
            var requisiteDto = mapper.Map<RequisiteDto>(requisite);
            await notificationService.NotifyRequisiteUpdated(requisiteDto);

            return mapper.Map<TransactionDto>(transactionEntity);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при обработке транзакции");
            return null;
        }
    }

    public async Task<List<TransactionDto>> GetAllTransactions()
    {
        var transactions = await unit.TransactionRepository.GetAllTransactions();
        return mapper.Map<List<TransactionDto>>(transactions);
    }

    public async Task<List<TransactionDto>> GetUserTransactions(Guid userId)
    {
        var transactions = await unit.TransactionRepository.GetUserTransactions(userId);
        return mapper.Map<List<TransactionDto>>(transactions);
    }
}