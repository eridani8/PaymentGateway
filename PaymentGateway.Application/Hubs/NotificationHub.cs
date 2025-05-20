using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Shared.DTOs.Chat;
using PaymentGateway.Shared.DTOs.User;
using Microsoft.AspNetCore.Authorization;

namespace PaymentGateway.Application.Hubs;

[Authorize(Policy = "Notification")]
public class NotificationHub(
    ILogger<NotificationHub> logger,
    IChatMessageService chatMessageService) : Hub<IWebClientHub>
{
    private static readonly ConcurrentDictionary<string, UserState> ConnectedUsers = new();
    
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

        return string.IsNullOrEmpty(user.Key) ? null : user.Key;
    }

    public Task<List<UserState>> GetAllUsers()
    {
        var result = ConnectedUsers
            .Values
            .DistinctBy(u => u.Id)
            .ToList();
        return Task.FromResult(result);
    }
    
    public async Task<List<ChatMessageDto>> GetChatMessages()
    {
        try
        {
            return await chatMessageService.GetAllChatMessages();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении истории сообщений");
            return [];
        }
    }

    public async Task SendChatMessage(string message)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(username))
        {
            var messageDto = new ChatMessageDto()
            {
                Id = Guid.CreateVersion7(),
                UserId = Guid.Parse(userId),
                Username = username,
                Timestamp = DateTime.UtcNow,
                Message = message
            };

            try
            {
                messageDto = await chatMessageService.SaveChatMessage(messageDto);
                await Clients.All.ChatMessageReceived(messageDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при сохранении сообщения в базу данных");
            }
        }
    }
}