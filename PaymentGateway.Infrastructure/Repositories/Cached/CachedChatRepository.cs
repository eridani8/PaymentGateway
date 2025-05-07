using Microsoft.Extensions.Caching.Memory;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Interfaces;

namespace PaymentGateway.Infrastructure.Repositories.Cached;

public class CachedChatRepository(ChatRepository repository, IMemoryCache cache) : IChatRepository
{
    private const string cacheKey = "ChatMessages";

    public async Task<List<ChatMessageEntity>> GetAllChatMessages()
    {
        var result = await cache.GetOrCreateAsync(cacheKey, entry =>
        {
            entry.SetSlidingExpiration(TimeSpan.FromHours(6));
            entry.SetAbsoluteExpiration(TimeSpan.FromHours(12));
            return repository.GetAllChatMessages();
        });
    
        return result ?? [];
    }

    public Task<ChatMessageEntity> AddChatMessage(ChatMessageEntity message)
    {
        cache.Remove(cacheKey);
        return repository.AddChatMessage(message);
    }
}