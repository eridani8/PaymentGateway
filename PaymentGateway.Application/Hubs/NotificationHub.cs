using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Shared.DTOs.Chat;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Interfaces;

namespace PaymentGateway.Application.Hubs;

public class NotificationHub(ILogger<NotificationHub> logger) : Hub<IHubClient>
{
    private static IHubContext<NotificationHub, IHubClient>? _hubContext;
    private static readonly ConcurrentDictionary<string, UserState> ConnectedUsers = new();
    private static readonly Timer? KeepAliveTimer;
    private const int KeepAliveInterval = 15000;

    static NotificationHub()
    {
        KeepAliveTimer = new Timer(SendKeepAlive, null, KeepAliveInterval, KeepAliveInterval);
    }

    private static async void SendKeepAlive(object? state)
    {
        try
        {
            if (_hubContext != null)
            {
                await _hubContext.Clients.All.KeepAlive();
            }
        }
        catch
        {
            // ignore
        }
    }

    public static void Initialize(IHubContext<NotificationHub, IHubClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            var roles = Context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? [];
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userName))
            {
                var state = new UserState()
                {
                    Id = Guid.Parse(userId),
                    Username = userName,
                    Roles = roles
                };

                ConnectedUsers[Context.ConnectionId] = state;

                if (GetUsersByRoles(["Admin", "Support"]) is { Count: > 0 } staffIds)
                {
                    await Clients.Clients(staffIds).UserConnected(state);
                }
            }

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при подключении клиента: {ConnectionId}", Context.ConnectionId);
            throw;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            if (ConnectedUsers.Remove(Context.ConnectionId, out var state))
            {
                if (GetUsersByRoles(["Admin", "Support"]) is { Count: > 0 } staffIds)
                {
                    await Clients.Clients(staffIds).UserDisconnected(state);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при отключении клиента: {ConnectionId}", Context.ConnectionId);
            throw;
        }
    }

    public static List<string> GetUsersByRoles(string[] roles)
    {
        var connectionIds = ConnectedUsers
            .Where(kvp => kvp.Value.Roles.Any(roles.Contains))
            .Select(kvp => kvp.Key)
            .ToList();

        return connectionIds;
    }

    public static string? GetUserConnectionId(Guid id)
    {
        var user = ConnectedUsers
            .FirstOrDefault(kvp => kvp.Value.Id == id);

        return user.Equals(default(KeyValuePair<string, UserState>)) ? null : user.Key;
    }

    public Task<List<UserState>> GetAdminsAndSupports()
    {
        var roles = new[] { "Admin", "Support" };

        return Task.FromResult(ConnectedUsers.Values
            .Where(u =>
                u.Roles.Any(r => roles.Contains(r)))
            .ToList());
    }

    [Authorize(Roles = "Admin,Support")]
    public async Task SendChatMessage(ChatMessageDto message)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            message.UserId = Guid.Parse(userId);
            message.Timestamp = DateTime.UtcNow;

            await Clients.All.ChatMessageReceived(message);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            KeepAliveTimer?.Dispose();
        }

        base.Dispose(disposing);
    }
}