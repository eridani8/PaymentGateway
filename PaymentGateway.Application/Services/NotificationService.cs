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
            logger.LogInformation("NotifyUserUpdated вызван для пользователя {UserId}", user.Id);
            
            var tasks = new List<Task>();
            
            var adminIds = NotificationHub.GetUsersByRoles(["Admin"]);
            
            if (adminIds.Count > 0)
            {
                var adminTask = hubContext.Clients.Clients(adminIds).UserUpdated(user);
                tasks.Add(adminTask);
                logger.LogInformation("Запущено уведомление об обновлении пользователя для {Count} администраторов, ConnectionIds: {ConnectionIds}", 
                    adminIds.Count, string.Join(", ", adminIds));
            }
            
            var broadcastTask = hubContext.Clients.All.UserUpdated(user);
            tasks.Add(broadcastTask);
            logger.LogInformation("Запущено широковещательное уведомление об обновлении пользователя {UserId}", user.Id);
            
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
            logger.LogInformation("NotifyUserDeleted вызван для пользователя {UserId}", id);
            
            var tasks = new List<Task>();
            
            var adminIds = NotificationHub.GetUsersByRoles(["Admin"]);
            
            if (adminIds.Count > 0)
            {
                var adminTask = hubContext.Clients.Clients(adminIds).UserDeleted(id);
                tasks.Add(adminTask);
                logger.LogInformation("Запущено уведомление об удалении пользователя для {Count} администраторов, ConnectionIds: {ConnectionIds}", 
                    adminIds.Count, string.Join(", ", adminIds));
            }
            
            var broadcastTask = hubContext.Clients.All.UserDeleted(id);
            tasks.Add(broadcastTask);
            logger.LogInformation("Запущено широковещательное уведомление об удалении пользователя {UserId}", id);
            
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
            logger.LogInformation("NotifyRequisiteUpdated вызван для реквизита {RequisiteId}", requisite.Id);
            
            var tasks = new List<Task>();
            
            var adminIds = NotificationHub.GetUsersByRoles(["Admin"]);
            
            if (adminIds.Count > 0)
            {
                var adminTask = hubContext.Clients.Clients(adminIds).RequisiteUpdated(requisite);
                tasks.Add(adminTask);
                logger.LogInformation("Запущено уведомление об обновлении реквизита для {Count} администраторов, ConnectionIds: {ConnectionIds}", 
                    adminIds.Count, string.Join(", ", adminIds));
            }
            
            var broadcastTask = hubContext.Clients.All.RequisiteUpdated(requisite);
            tasks.Add(broadcastTask);
            logger.LogInformation("Запущено широковещательное уведомление об обновлении реквизита {RequisiteId}", requisite.Id);
            
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
            logger.LogInformation("NotifyRequisiteDeleted вызван для реквизита {RequisiteId}", requisiteId);
            
            var tasks = new List<Task>();
            
            var adminIds = NotificationHub.GetUsersByRoles(["Admin"]);
            
            if (adminIds.Count > 0)
            {
                var adminTask = hubContext.Clients.Clients(adminIds).RequisiteDeleted(requisiteId);
                tasks.Add(adminTask);
                logger.LogInformation("Запущено уведомление об удалении реквизита для {Count} администраторов, ConnectionIds: {ConnectionIds}", 
                    adminIds.Count, string.Join(", ", adminIds));
            }
            
            var broadcastTask = hubContext.Clients.All.RequisiteDeleted(requisiteId);
            tasks.Add(broadcastTask);
            logger.LogInformation("Запущено широковещательное уведомление об удалении реквизита {RequisiteId}", requisiteId);
            
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
            logger.LogInformation("NotifyPaymentUpdated вызван для платежа {PaymentId}", payment.Id);
            
            var tasks = new List<Task>();
            
            var staffIds = NotificationHub.GetUsersByRoles(["Admin", "Support"]);
            
            if (staffIds.Count > 0)
            {
                var staffTask = hubContext.Clients.Clients(staffIds).PaymentUpdated(payment);
                tasks.Add(staffTask);
                logger.LogInformation("Запущено уведомление об обновлении платежа для {Count} сотрудников, ConnectionIds: {ConnectionIds}", 
                    staffIds.Count, string.Join(", ", staffIds));
            }
            
            var broadcastTask = hubContext.Clients.All.PaymentUpdated(payment);
            tasks.Add(broadcastTask);
            logger.LogInformation("Запущено широковещательное уведомление об обновлении платежа {PaymentId}", payment.Id);
            
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
            logger.LogInformation("NotifyPaymentDeleted вызван для платежа {PaymentId}", id);
            
            var tasks = new List<Task>();
            
            var staffIds = NotificationHub.GetUsersByRoles(["Admin", "Support"]);
            
            if (staffIds.Count > 0)
            {
                var staffTask = hubContext.Clients.Clients(staffIds).PaymentDeleted(id);
                tasks.Add(staffTask);
                logger.LogInformation("Запущено уведомление об удалении платежа для {Count} сотрудников, ConnectionIds: {ConnectionIds}", 
                    staffIds.Count, string.Join(", ", staffIds));
            }
            
            var broadcastTask = hubContext.Clients.All.PaymentDeleted(id);
            tasks.Add(broadcastTask);
            logger.LogInformation("Запущено широковещательное уведомление об удалении платежа {PaymentId}", id);
            
            await Task.WhenAll(tasks);
            
            logger.LogInformation("Все уведомления об удалении платежа {PaymentId} отправлены успешно", id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка отправки уведомления об удалении платежа {PaymentId}", id);
        }
    }
} 