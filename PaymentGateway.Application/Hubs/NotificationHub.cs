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
    private static Timer? _keepAliveTimer;
    private const int KeepAliveInterval = 25000;

    static NotificationHub()
    {
        _keepAliveTimer = new Timer(SendKeepAlive, null, KeepAliveInterval, KeepAliveInterval);
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

    private static IHubContext<NotificationHub, IHubClient>? _hubContext;

    public static void Initialize(IHubContext<NotificationHub, IHubClient> hubContext)
    {
        _hubContext = hubContext;
    }

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

    public Task Ping()
    {
        return Task.CompletedTask;
    }
    
    [Authorize(Roles = "Admin")]
    public Dictionary<string, int> GetStats()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        logger.LogInformation("Статистика запрошена пользователем: {UserId}", userId);
        return GetConnectionStats();
    }
    
    [Authorize(Roles = "Admin")]
    public Dictionary<string, List<string>> GetCurrentUsers()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        logger.LogInformation("Список подключенных пользователей запрошен пользователем: {UserId}", userId);
        return GetConnectedUsers();
    }
    
    public static List<string> GetGroupIds(string[] roles)
    {
        return UserRoles
            .Where(kvp => kvp.Value.Any(role => roles.Contains(role)))
            .Select(kvp => kvp.Key)
            .ToList();
    }
    
    public static List<string> GetUsersByRoles(string[] roles)
    {
        return UserRoles
            .Where(kvp => kvp.Value.Any(roles.Contains))
            .Select(kvp => kvp.Key)
            .ToList();
    }
    
    public static Dictionary<string, List<string>> GetConnectedUsers()
    {
        return new Dictionary<string, List<string>>(UserRoles);
    }
    
    public static int GetConnectionCount()
    {
        return ConnectedUsers.Count;
    }
    
    public static Dictionary<string, int> GetConnectionStats()
    {
        var stats = new Dictionary<string, int>
        {
            ["total"] = ConnectedUsers.Count
        };
        
        foreach (var role in UserRoles.Values.SelectMany(r => r).Distinct())
        {
            stats[role] = UserRoles.Count(kvp => kvp.Value.Contains(role));
        }
        
        return stats;
    }
    
    public async Task RegisterForPaymentUpdates(Guid paymentId)
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var connectionId = Context.ConnectionId;
            var groupName = $"PaymentUpdate_{paymentId}";
            
            await Groups.AddToGroupAsync(connectionId, groupName);
            
            logger.LogInformation("Клиент {ConnectionId} (пользователь {UserId}) зарегистрирован на обновления платежа {PaymentId}", 
                connectionId, userId, paymentId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при регистрации клиента на обновления платежа {PaymentId}", paymentId);
            throw;
        }
    }

    public async Task UnregisterFromPaymentUpdates(Guid paymentId)
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var connectionId = Context.ConnectionId;
            var groupName = $"PaymentUpdate_{paymentId}";
            
            await Groups.RemoveFromGroupAsync(connectionId, groupName);
            
            logger.LogInformation("Клиент {ConnectionId} (пользователь {UserId}) отписан от обновлений платежа {PaymentId}", 
                connectionId, userId, paymentId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при отписке клиента от обновлений платежа {PaymentId}", paymentId);
            throw;
        }
    }
} 