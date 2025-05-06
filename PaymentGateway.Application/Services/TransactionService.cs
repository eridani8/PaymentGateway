using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Results;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Interfaces;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.Transaction;

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
    public async Task<Result<TransactionDto>> CreateTransaction(TransactionCreateDto dto)
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            return Result.Failure<TransactionDto>(new ValidationError(validation.Errors.Select(e => e.ErrorMessage)));
        }

        var requisite = await unit.RequisiteRepository.GetRequisiteByPaymentData(dto.PaymentData);

        if (requisite?.Payment is not { } payment)
        {
            return Result.Failure<TransactionDto>(RequisiteErrors.RequisiteNotFound);
        }

        var transactionEntity = mapper.Map<TransactionEntity>(dto);

        logger.LogInformation("Поступление платежа на сумму {Amount}", transactionEntity.ExtractedAmount);
        
        if (payment.Amount != dto.ExtractedAmount)
        {
            await unit.TransactionRepository.Add(transactionEntity);
            await unit.Commit();
            return Result.Failure<TransactionDto>(TransactionErrors.WrongPaymentAmount(dto.ExtractedAmount, payment.Amount));
        }

        payment.ConfirmTransaction(transactionEntity);
        await unit.TransactionRepository.Add(transactionEntity);
        await paymentConfirmationService.ProcessPaymentConfirmation(payment, requisite, dto.ExtractedAmount);

        await unit.Commit();

        var paymentDto = mapper.Map<PaymentDto>(payment);
        await notificationService.NotifyPaymentUpdated(paymentDto);
        var requisiteDto = mapper.Map<RequisiteDto>(requisite);
        await notificationService.NotifyRequisiteUpdated(requisiteDto);

        return Result.Success(mapper.Map<TransactionDto>(transactionEntity));
    }

    public async Task<Result<List<TransactionDto>>> GetAllTransactions()
    {
        var transactions = await unit.TransactionRepository.GetAllTransactions();
        return Result.Success(mapper.Map<List<TransactionDto>>(transactions));
    }

    public async Task<Result<List<TransactionDto>>> GetUserTransactions(Guid userId)
    {
        var transactions = await unit.TransactionRepository.GetUserTransactions(userId);
        return Result.Success(mapper.Map<List<TransactionDto>>(transactions));
    }
}