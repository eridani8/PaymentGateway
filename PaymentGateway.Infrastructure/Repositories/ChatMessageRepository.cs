using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Core.Interfaces.Repositories;
using PaymentGateway.Infrastructure.Data;
using System.Text.Json;

namespace PaymentGateway.Infrastructure.Repositories;

public class ChatMessageRepository(
    AppDbContext context,
    ICache cache,
    ILogger<ChatMessageRepository> logger,
    JsonSerializerOptions options)
    : RepositoryBase<ChatMessageEntity>(context), IChatMessageRepository
{
    private readonly AppDbContext _context = context;

    private const string ChatMessagesCacheKey = "ChatMessages:All";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(12);

    public async Task<List<ChatMessageEntity>> GetAllChatMessages()
    {
        var cachedValue = cache.GetString(ChatMessagesCacheKey);
        if (!string.IsNullOrEmpty(cachedValue))
        {
            try
            {
                return JsonSerializer.Deserialize<List<ChatMessageEntity>>(cachedValue, options) ?? [];
            }
            catch
            {
                cache.Remove(ChatMessagesCacheKey);
            }
        }

        var messages = await _context.ChatMessages
            .Include(m => m.User)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        try
        {
            var jsonMessages = JsonSerializer.Serialize(messages, options);
            cache.SetString(ChatMessagesCacheKey, jsonMessages, CacheDuration);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при кешировании сообщений чата");
        }

        return messages;
    }

    public async Task<ChatMessageEntity> AddChatMessage(ChatMessageEntity message)
    {
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        var cachedValue = cache.GetString(ChatMessagesCacheKey);
        if (!string.IsNullOrEmpty(cachedValue))
        {
            try
            {
                var cachedMessages = JsonSerializer.Deserialize<List<ChatMessageEntity>>(cachedValue, options) ?? [];

                cachedMessages.Add(message);

                var remainingLifetime = cache.GetRemainingLifetime(ChatMessagesCacheKey) ?? CacheDuration;
                var jsonMessages = JsonSerializer.Serialize(cachedMessages, options);
                cache.SetString(ChatMessagesCacheKey, jsonMessages, remainingLifetime);

                logger.LogDebug("Обновлен кеш сообщений чата, добавлено новое сообщение. Осталось времени жизни: {0}", remainingLifetime);
            }
            catch (Exception e)
            {
                cache.Remove(ChatMessagesCacheKey);
                logger.LogWarning(e, "Не удалось обновить кеш сообщений чата");
            }
        }

        return message;
    }
}