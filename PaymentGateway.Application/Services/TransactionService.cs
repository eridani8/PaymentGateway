using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Results;
using PaymentGateway.Core.Configs;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Interfaces;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.Transaction;
using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Application.Services;

public class TransactionService(
    IUnitOfWork unit,
    IMapper mapper,
    IValidator<TransactionCreateDto> validator,
    ILogger<TransactionService> logger,
    IPaymentConfirmationService paymentConfirmationService,
    INotificationService notificationService,
    GatewaySettings gatewaySettings,
    UserManager<UserEntity> userManager)
    : ITransactionService
{
    public async Task<Result<TransactionDto>> CreateTransaction(TransactionCreateDto dto)
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            return Result.Failure<TransactionDto>(new ValidationError(validation.Errors.Select(e => e.ErrorMessage)));
        }

        var requisite = await unit.RequisiteRepository.GetRequisiteById(dto.RequisiteId);

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

        var exchangeAmount = payment.Amount / gatewaySettings.UsdtExchangeRate;
        
        var requisiteUser = requisite.User;
        requisiteUser.Profit += exchangeAmount * 0.05m;
        await userManager.UpdateAsync(requisiteUser);
        await notificationService.NotifyWalletUpdated(mapper.Map<WalletDto>(requisiteUser));
        // TODO profit
        var paymentUser = payment.User;
        paymentUser.Frozen -= exchangeAmount;
        await userManager.UpdateAsync(paymentUser);
        await notificationService.NotifyWalletUpdated(mapper.Map<WalletDto>(paymentUser));

        var paymentDto = mapper.Map<PaymentDto>(payment);
        var requisiteDto = mapper.Map<RequisiteDto>(requisite);
        
        await notificationService.NotifyPaymentUpdated(paymentDto);
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