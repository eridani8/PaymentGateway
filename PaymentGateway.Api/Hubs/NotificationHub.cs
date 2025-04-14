using Microsoft.AspNetCore.SignalR;
using PaymentGateway.Shared.Interfaces;

namespace PaymentGateway.Api.Hubs;

public class NotificationHub(ILogger<NotificationHub> logger) : Hub<IHubClient>
{
    public override async Task OnConnectedAsync()
    {
        logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
} 