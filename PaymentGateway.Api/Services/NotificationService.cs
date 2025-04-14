using Microsoft.AspNetCore.SignalR;
using PaymentGateway.Api.Hubs;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Interfaces;
using System.Security.Claims;

namespace PaymentGateway.Api.Services;

public class NotificationService(
    IHubContext<NotificationHub, IHubClient> hubContext,
    ILogger<NotificationService> logger)
    : INotificationService
{
    public async Task NotifyUserUpdated(UserDto user)
    {
        try
        {
            await hubContext.Clients.All.UserUpdated(user);
            logger.LogInformation("User updated notification sent for user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending user updated notification for user {UserId}", user.Id);
        }
    }

    public async Task NotifyPaymentUpdated(PaymentDto payment)
    {
        try
        {
            await hubContext.Clients.All.PaymentUpdated(payment);
            logger.LogInformation("Payment updated notification sent for payment {PaymentId}", payment.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending payment updated notification for payment {PaymentId}", payment.Id);
        }
    }

    public async Task NotifyRequisiteUpdated(RequisiteDto requisite)
    {
        try
        {
            await hubContext.Clients.User(requisite.UserId.ToString()).RequisiteUpdated(requisite);
            
            var rootUserIds = NotificationHub.GetRootUserIds();
            if (rootUserIds.Count > 0)
            {
                await hubContext.Clients.Users(rootUserIds).RequisiteUpdated(requisite);
            }
            
            logger.LogInformation("Requisite updated notification sent for requisite {RequisiteId}", requisite.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending requisite updated notification for requisite {RequisiteId}", requisite.Id);
        }
    }

    public async Task NotifyRequisiteDeleted(Guid requisiteId, Guid userId)
    {
        try
        {
            await hubContext.Clients.User(userId.ToString()).RequisiteDeleted(requisiteId);
            
            var rootUserIds = NotificationHub.GetRootUserIds();
            if (rootUserIds.Count > 0)
            {
                await hubContext.Clients.Users(rootUserIds).RequisiteDeleted(requisiteId);
            }
            
            logger.LogInformation("Requisite deleted notification sent for requisite {RequisiteId}", requisiteId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending requisite deleted notification for requisite {RequisiteId}", requisiteId);
        }
    }

    public async Task NotifyPaymentStatusChanged(PaymentDto payment)
    {
        try
        {
            await hubContext.Clients.All.PaymentStatusChanged(payment);
            logger.LogInformation("Payment status changed notification sent for payment {PaymentId}", payment.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending payment status changed notification for payment {PaymentId}", payment.Id);
        }
    }
} 