using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Shared.Interfaces;

namespace PaymentGateway.Application.Hubs;

public class NotificationHub(ILogger<NotificationHub> logger) : Hub<IHubClient>
{
    private static readonly Dictionary<string, string> ConnectedUsers = new();
    private static readonly Dictionary<string, List<string>> UserRoles = new();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            ConnectedUsers[Context.ConnectionId] = userId;
            
            var roles = Context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? [];
            UserRoles[userId] = roles;
        }

        logger.LogInformation("Client connected: {ConnectionId}, UserId: {UserId}", Context.ConnectionId, userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (ConnectedUsers.Remove(Context.ConnectionId, out var userId))
        {
            UserRoles.Remove(userId);
        }

        logger.LogInformation("Client disconnected: {ConnectionId}, UserId: {UserId}", Context.ConnectionId, userId);
        await base.OnDisconnectedAsync(exception);
    }

    public static List<string> GetRootUserIds()
    {
        return UserRoles
            .Where(kvp => kvp.Value.Contains("Admin") && kvp.Value.Contains("Support") && kvp.Value.Contains("User"))
            .Select(kvp => kvp.Key)
            .ToList();
    }
} 