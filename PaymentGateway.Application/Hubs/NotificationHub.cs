using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                ConnectedUsers[Context.ConnectionId] = userId;
                
                var roles = Context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? [];
                UserRoles[userId] = roles;
                
                logger.LogInformation("Клиент подключен: {ConnectionId}, Пользователь: {UserId}, Роли: {Roles}", 
                    Context.ConnectionId, userId, string.Join(", ", roles));
            }
            else
            {
                logger.LogWarning("Подключение клиента без идентификатора пользователя: {ConnectionId}", Context.ConnectionId);
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
            if (ConnectedUsers.Remove(Context.ConnectionId, out var userId))
            {
                UserRoles.Remove(userId);
                logger.LogInformation("Клиент отключен: {ConnectionId}, Пользователь: {UserId}", Context.ConnectionId, userId);
            }
            else
            {
                logger.LogInformation("Клиент отключен (неизвестный пользователь): {ConnectionId}", Context.ConnectionId);
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
        return UserRoles
            .Where(kvp => kvp.Value.Any(roles.Contains))
            .Select(kvp => kvp.Key)
            .ToList();
    }
} 