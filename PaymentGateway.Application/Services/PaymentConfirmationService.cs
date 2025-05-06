using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Interfaces;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Application.Services;

public class PaymentConfirmationService(
    IUnitOfWork unit,
    ILogger<PaymentConfirmationService> logger,
    UserManager<UserEntity> userManager) : IPaymentConfirmationService
{
    public async Task ProcessPaymentConfirmation(PaymentEntity payment, RequisiteEntity requisite, decimal amount)
    {
        requisite.ReleaseAfterPayment(amount, out var status);
        logger.LogInformation("Освобождение реквизита {RequisiteId}", requisite.Id);
        if (status == RequisiteStatus.Cooldown)
        {
            logger.LogInformation("Статус реквизита {Status} {RequisiteId} на {Seconds} сек.", status, requisite.Id, (int)requisite.Cooldown.TotalSeconds);   
        }
        
        requisite.User.ReceivedDailyFunds += amount;
        logger.LogInformation("Увеличение суточных поступлений пользователя {UserId} на {Amount}, текущая сумма: {CurrentDailyReceived}", 
            requisite.User.Id, amount, requisite.User.ReceivedDailyFunds);
        await userManager.UpdateAsync(requisite.User);
        
        requisite.PaymentId = null;
        requisite.Payment = null;
        
        unit.RequisiteRepository.Update(requisite);
        unit.PaymentRepository.Update(payment);
    }
} 