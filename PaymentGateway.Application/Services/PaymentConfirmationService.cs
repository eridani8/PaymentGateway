using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.Enums;
using PaymentGateway.Shared.Interfaces;

namespace PaymentGateway.Application.Services;

public class PaymentConfirmationService(
    IUnitOfWork unit,
    IMapper mapper,
    ILogger<PaymentConfirmationService> logger,
    UserManager<UserEntity> userManager,
    INotificationService notificationService) : IPaymentConfirmationService
{
    public async Task ProcessPaymentConfirmation(PaymentEntity payment, RequisiteEntity requisite, decimal amount)
    {
        requisite.ReleaseAfterPayment(amount, out var status);
        logger.LogInformation("Освобождение реквизита {requisiteId}", requisite.Id);
        if (status == RequisiteStatus.Cooldown)
        {
            logger.LogInformation("Статус реквизита {status} {requisiteId} на {sec} сек.", status, requisite.Id, (int)requisite.Cooldown.TotalSeconds);   
        }
        
        requisite.User.ReceivedDailyFunds += amount;
        logger.LogInformation("Увеличение суточных поступлений пользователя {userId} на {amount}, текущая сумма: {total}", 
            requisite.User.Id, amount, requisite.User.ReceivedDailyFunds);
        await userManager.UpdateAsync(requisite.User);
        
        requisite.PaymentId = null;
        requisite.Payment = null;
        
        unit.RequisiteRepository.Update(requisite);
        unit.PaymentRepository.Update(payment);
        
        await unit.Commit();

        var paymentDto = mapper.Map<PaymentDto>(payment);
        await notificationService.NotifyPaymentUpdated(paymentDto);
        await notificationService.NotifySpecificPaymentUpdated(paymentDto);
        
        var requisiteDto = mapper.Map<RequisiteDto>(requisite);
        
        await notificationService.NotifyRequisiteUpdated(requisiteDto);
    }
} 