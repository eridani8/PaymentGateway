using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Hubs;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Enums;

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
            
            if (NotificationHub.GetUsersByRoles(["Admin"]) is { Count: > 0 } adminIds)
            {
                var adminTask = hubContext.Clients.Clients(adminIds).UserUpdated(user);
                tasks.Add(adminTask);
            }

            if (NotificationHub.GetUserConnectionId(user.Id) is { } connectionId)
            {
                var userTask = hubContext.Clients.Client(connectionId).UserUpdated(user);
                tasks.Add(userTask);
            }
            
            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка отправки уведомления об обновлении пользователя {UserId}", user.Id);
        }
    }

    public async Task NotifyUserDeleted(Guid id)
    {
        try
        {
            var tasks = new List<Task>();
            
            if (NotificationHub.GetUsersByRoles(["Admin"]) is { Count: > 0 } adminIds)
            {
                var adminTask = hubContext.Clients.Clients(adminIds).UserDeleted(id);
                tasks.Add(adminTask);
            }
            
            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка отправки уведомления об удалении пользователя {UserId}", id);
        }
    }
    
    public async Task NotifyRequisiteUpdated(RequisiteDto requisite)
    {
        try
        {
            var tasks = new List<Task>();
            
            if (NotificationHub.GetUsersByRoles(["Admin"]) is { Count: > 0 } adminIds)
            {
                var adminTask = hubContext.Clients.Clients(adminIds).RequisiteUpdated(requisite);
                tasks.Add(adminTask);
            }

            if (NotificationHub.GetUserConnectionId(requisite.UserId) is { } connectionId)
            {
                var userTask = hubContext.Clients.Client(connectionId).RequisiteUpdated(requisite);
                tasks.Add(userTask);
            }
            
            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка отправки уведомления об обновлении реквизита {RequisiteId}", requisite.Id);
        }
    }

    public async Task NotifyRequisiteDeleted(Guid requisiteId, Guid userId)
    {
        try
        {
            var tasks = new List<Task>();
            
            if (NotificationHub.GetUsersByRoles(["Admin"]) is { Count: > 0 } adminIds)
            {
                var adminTask = hubContext.Clients.Clients(adminIds).RequisiteDeleted(requisiteId);
                tasks.Add(adminTask);
            }

            if (NotificationHub.GetUserConnectionId(userId) is { } connectionId)
            {
                var userTask = hubContext.Clients.Client(connectionId).RequisiteDeleted(requisiteId);
                tasks.Add(userTask);
            }
            
            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка отправки уведомления об удалении реквизита {RequisiteId}", requisiteId);
        }
    }

    public async Task NotifyPaymentUpdated(PaymentDto payment)
    {
        try
        {
            var tasks = new List<Task>();
            
            if (NotificationHub.GetUsersByRoles(["Admin", "Support"]) is { Count: > 0 } staffIds)
            {
                var staffTask = hubContext.Clients.Clients(staffIds).PaymentUpdated(payment);
                tasks.Add(staffTask);
            }

            if (payment.Requisite != null && NotificationHub.GetUserConnectionId(payment.Requisite.UserId) is { } connectionId)
            {
                var userTask = hubContext.Clients.Client(connectionId).PaymentUpdated(payment);
                tasks.Add(userTask);
            }
            
            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка отправки уведомления об обновлении платежа {PaymentId}", payment.Id);
        }
    }

    public async Task NotifyPaymentDeleted(Guid id, Guid? userId)
    {
        try
        {
            var tasks = new List<Task>();
            
            if (NotificationHub.GetUsersByRoles(["Admin", "Support"]) is { Count: > 0 } staffIds)
            {
                var staffTask = hubContext.Clients.Clients(staffIds).PaymentDeleted(id);
                tasks.Add(staffTask);
            }

            if (userId != null && NotificationHub.GetUserConnectionId(userId.Value) is { } connectionId)
            {
                var userTask = hubContext.Clients.Client(connectionId).PaymentDeleted(id);
                tasks.Add(userTask);
            }
            
            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка отправки уведомления об удалении платежа {PaymentId}", id);
        }
    }

    public async Task NotifyRequisiteAssignmentAlgorithmChanged(RequisiteAssignmentAlgorithm algorithm)
    {
        try
        {
            var tasks = new List<Task>();
            
            if (NotificationHub.GetUsersByRoles(["Admin"]) is { Count: > 0 } staffIds)
            {
                var staffTask = hubContext.Clients.Clients(staffIds).ChangeRequisiteAssignmentAlgorithm(algorithm);
                tasks.Add(staffTask);
            }
            
            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка отправки уведомления об изменении алгоритма подбора реквизитов");
        }
    }
} 