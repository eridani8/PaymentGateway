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
            var tasks = new List<Task>();
            
            var adminIds = NotificationHub.GetUsersByRoles(["Admin"]);
            if (adminIds.Count > 0)
            {
                var adminTask = hubContext.Clients.Users(adminIds).UserUpdated(user);
                tasks.Add(adminTask);
                logger.LogInformation("Запущено уведомление об обновлении пользователя для {Count} администраторов, пользователь {UserId}", 
                    adminIds.Count, user.Id);
            }
            
            var userTask = hubContext.Clients.User(user.Id.ToString()).UserUpdated(user);
            tasks.Add(userTask);
            logger.LogInformation("Запущено уведомление об обновлении для самого пользователя {UserId}", user.Id);
            
            await Task.WhenAll(tasks);
            
            logger.LogInformation("Все уведомления об обновлении пользователя {UserId} отправлены успешно", user.Id);
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
            var tasks = new List<Task>();
            
            var adminIds = NotificationHub.GetUsersByRoles(["Admin"]);
            if (adminIds.Count > 0)
            {
                var adminTask = hubContext.Clients.Users(adminIds).UserDeleted(id);
                tasks.Add(adminTask);
                logger.LogInformation("Запущено уведомление об удалении пользователя для {Count} администраторов, пользователь {UserId}", 
                    adminIds.Count, id);
            }
            
            await Task.WhenAll(tasks);
            
            logger.LogInformation("Все уведомления об удалении пользователя {UserId} отправлены успешно", id);
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
            var tasks = new List<Task>();
            
            var ownerTask = hubContext.Clients.User(requisite.UserId.ToString()).RequisiteUpdated(requisite);
            tasks.Add(ownerTask);
            logger.LogInformation("Запущено уведомление об обновлении реквизита для владельца {UserId}, реквизит {RequisiteId}", 
                requisite.UserId, requisite.Id);
            
            var adminIds = NotificationHub.GetUsersByRoles(["Admin"]);
            if (adminIds.Count > 0)
            {
                var adminTask = hubContext.Clients.Users(adminIds).RequisiteUpdated(requisite);
                tasks.Add(adminTask);
                logger.LogInformation("Запущено уведомление об обновлении реквизита для {Count} администраторов, реквизит {RequisiteId}", 
                    adminIds.Count, requisite.Id);
            }
            
            if (requisite.Payment != null)
            {
                logger.LogInformation("Реквизит {RequisiteId} имеет назначенный платеж {PaymentId}, отправляем широковещательное уведомление", 
                    requisite.Id, requisite.Payment.Id);
                var broadcastTask = hubContext.Clients.All.RequisiteUpdated(requisite);
                tasks.Add(broadcastTask);
            }
            
            await Task.WhenAll(tasks);
            
            logger.LogInformation("Все уведомления об обновлении реквизита {RequisiteId} отправлены успешно", requisite.Id);
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
            var tasks = new List<Task>();
            
            var ownerTask = hubContext.Clients.User(userId.ToString()).RequisiteDeleted(requisiteId);
            tasks.Add(ownerTask);
            logger.LogInformation("Запущено уведомление об удалении реквизита для владельца {UserId}, реквизит {RequisiteId}", 
                userId, requisiteId);
            
            var adminIds = NotificationHub.GetUsersByRoles(["Admin"]);
            if (adminIds.Count > 0)
            {
                var adminTask = hubContext.Clients.Users(adminIds).RequisiteDeleted(requisiteId);
                tasks.Add(adminTask);
                logger.LogInformation("Запущено уведомление об удалении реквизита для {Count} администраторов, реквизит {RequisiteId}", 
                    adminIds.Count, requisiteId);
            }
            
            await Task.WhenAll(tasks);
            
            logger.LogInformation("Все уведомления об удалении реквизита {RequisiteId} отправлены успешно", requisiteId);
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
            var tasks = new List<Task>();
            
            if (payment.Requisite?.UserId.ToString() is { } userId)
            {
                var ownerTask = hubContext.Clients.User(userId).PaymentUpdated(payment);
                tasks.Add(ownerTask);
                logger.LogInformation("Запущено уведомление об обновлении платежа для владельца {UserId}, платеж {PaymentId}", 
                    userId, payment.Id);
            }
            
            var staffIds = NotificationHub.GetUsersByRoles(["Admin", "Support"]);
            if (staffIds.Count > 0)
            {
                var staffTask = hubContext.Clients.Users(staffIds).PaymentUpdated(payment);
                tasks.Add(staffTask);
                logger.LogInformation("Запущено уведомление об обновлении платежа для {Count} сотрудников, платеж {PaymentId}", 
                    staffIds.Count, payment.Id);
            }
            
            var broadcastTask = hubContext.Clients.All.PaymentUpdated(payment);
            tasks.Add(broadcastTask);
            logger.LogInformation("Запущено широковещательное уведомление об обновлении платежа {PaymentId}", payment.Id);
            
            var specificGroupName = $"PaymentUpdate_{payment.Id}";
            var specificTask = hubContext.Clients.Group(specificGroupName).PaymentUpdated(payment);
            tasks.Add(specificTask);
            logger.LogInformation("Запущено уведомление для группы подписчиков платежа {PaymentId}", payment.Id);
            
            await Task.WhenAll(tasks);
            
            logger.LogInformation("Все уведомления об обновлении платежа {PaymentId} отправлены успешно", payment.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка отправки уведомления об обновлении платежа {PaymentId}", payment.Id);
        }
    }

    public async Task NotifyPaymentDeleted(Guid id, Guid? userId)
    {
        try
        {
            var tasks = new List<Task>();

            if (userId.HasValue)
            {
                var ownerTask = hubContext.Clients.User(userId.Value.ToString()).PaymentDeleted(id);
                tasks.Add(ownerTask);
                logger.LogInformation("Запущено уведомление об удалении платежа для владельца {UserId}, платеж {PaymentId}", userId, id);
            }
            
            var staffIds = NotificationHub.GetUsersByRoles(["Admin", "Support"]);
            if (staffIds.Count > 0)
            {
                var staffTask = hubContext.Clients.Users(staffIds).PaymentDeleted(id);
                tasks.Add(staffTask);
                logger.LogInformation("Запущено уведомление об удалении платежа для {Count} сотрудников, платеж {PaymentId}", 
                    staffIds.Count, id);
            }
            
            await Task.WhenAll(tasks);
            
            logger.LogInformation("Все уведомления об удалении платежа {PaymentId} отправлены успешно", id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка отправки уведомления об удалении платежа {PaymentId}", id);
        }
    }

    public async Task NotifySpecificPaymentUpdated(PaymentDto payment)
    {
        try
        {
            var groupName = $"PaymentUpdate_{payment.Id}";
            await hubContext.Clients.Group(groupName).PaymentUpdated(payment);
            logger.LogInformation("Отправлено уведомление об обновлении платежа {PaymentId} для группы {GroupName}", 
                payment.Id, groupName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка отправки уведомления об обновлении платежа {PaymentId} для группы", payment.Id);
        }
    }
} 