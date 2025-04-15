using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Core.Interfaces;
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
    INotificationService notificationService,
    UserManager<UserEntity> userManager)
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

        requisite.User.ReceivedDailyFunds += dto.ExtractedAmount;
        logger.LogInformation("Увеличение суточных поступлений пользователя {userId} на {amount}, текущая сумма: {total}", requisite.User.Id, dto.ExtractedAmount, requisite.User.ReceivedDailyFunds);
        await userManager.UpdateAsync(requisite.User);

        unit.RequisiteRepository.Update(requisite);
        await unit.TransactionRepository.Add(transaction);
        await unit.Commit();
        
        await notificationService.NotifyRequisiteUpdated(mapper.Map<RequisiteDto>(requisite));

        return mapper.Map<TransactionDto>(transaction);
    }
}