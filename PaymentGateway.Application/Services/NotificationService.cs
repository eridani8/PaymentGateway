using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Hubs;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Interfaces;

namespace PaymentGateway.Application.Services;

public class NotificationService(
    IHubContext<NotificationHub, IHubClient> hubContext,
    ILogger<NotificationService> logger)
    : INotificationService
{
    public async Task NotifyUserUpdated(UserDto user)
    {
        try
        {
            var adminIds = NotificationHub.GetUsersByRoles(["Admin"]);
            if (adminIds.Count > 0)
            {
                await hubContext.Clients.Users(adminIds).UserUpdated(user);
                logger.LogInformation("Уведомление об обновлении пользователя отправлено {Count} администраторам для пользователя {UserId}", 
                    adminIds.Count, user.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка отправки уведомления об обновлении пользователя {UserId}", user.Id);
        }
    }

    public async Task NotifyUserDeleted(Guid id)
    {
        try
        {
            var adminIds = NotificationHub.GetUsersByRoles(["Admin"]);
            if (adminIds.Count > 0)
            {
                await hubContext.Clients.Users(adminIds).UserDeleted(id);
                logger.LogInformation("Уведомление об удалении пользователя отправлено {Count} администраторам для пользователя {UserId}", 
                    adminIds.Count, id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка отправки уведомления об удалении пользователя {UserId}", id);
        }
    }
    
    public async Task NotifyRequisiteUpdated(RequisiteDto requisite)
    {
        try
        {
            await hubContext.Clients.User(requisite.UserId.ToString()).RequisiteUpdated(requisite);
            logger.LogInformation("Уведомление об обновлении реквизита отправлено владельцу {UserId} для реквизита {RequisiteId}", 
                requisite.UserId, requisite.Id);
            
            var adminIds = NotificationHub.GetUsersByRoles(["Admin"]);
            if (adminIds.Count > 0)
            {
                await hubContext.Clients.Users(adminIds).RequisiteUpdated(requisite);
                logger.LogInformation("Уведомление об обновлении реквизита отправлено {Count} администраторам для реквизита {RequisiteId}", 
                    adminIds.Count, requisite.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка отправки уведомления об обновлении реквизита {RequisiteId}", requisite.Id);
        }
    }

    public async Task NotifyRequisiteDeleted(Guid requisiteId, Guid userId)
    {
        try
        {
            await hubContext.Clients.User(userId.ToString()).RequisiteDeleted(requisiteId);
            logger.LogInformation("Уведомление об удалении реквизита отправлено владельцу {UserId} для реквизита {RequisiteId}", 
                userId, requisiteId);
            
            var adminIds = NotificationHub.GetUsersByRoles(["Admin"]);
            if (adminIds.Count > 0)
            {
                await hubContext.Clients.Users(adminIds).RequisiteDeleted(requisiteId);
                logger.LogInformation("Уведомление об удалении реквизита отправлено {Count} администраторам для реквизита {RequisiteId}", 
                    adminIds.Count, requisiteId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка отправки уведомления об удалении реквизита {RequisiteId}", requisiteId);
        }
    }

    public async Task NotifyPaymentUpdated(PaymentDto payment)
    {
        try
        {
            if (payment.Requisite?.UserId.ToString() is { } userId)
            {
                await hubContext.Clients.User(userId).PaymentUpdated(payment);
                logger.LogInformation("Уведомление об обновлении платежа отправлено владельцу {UserId} для платежа {PaymentId}", 
                    userId, payment.Id);
            }
            
            var staffIds = NotificationHub.GetUsersByRoles(["Admin", "Support"]);
            if (staffIds.Count > 0)
            {
                await hubContext.Clients.Users(staffIds).PaymentUpdated(payment);
                logger.LogInformation("Уведомление об обновлении платежа отправлено {Count} сотрудникам для платежа {PaymentId}", 
                    staffIds.Count, payment.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка отправки уведомления об обновлении платежа {PaymentId}", payment.Id);
        }
    }

    public async Task NotifyPaymentDeleted(Guid id, Guid userId)
    {
        try
        {
            await hubContext.Clients.User(userId.ToString()).PaymentDeleted(id);
            logger.LogInformation("Уведомление об удалении платежа отправлено владельцу {UserId} для платежа {PaymentId}", 
                userId, id);
            
            var staffIds = NotificationHub.GetUsersByRoles(["Admin", "Support"]);
            if (staffIds.Count > 0)
            {
                await hubContext.Clients.Users(staffIds).PaymentDeleted(id);
                logger.LogInformation("Уведомление об удалении платежа отправлено {Count} сотрудникам для платежа {PaymentId}", 
                    staffIds.Count, id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка отправки уведомления об удалении платежа {PaymentId}", id);
        }
    }
} 