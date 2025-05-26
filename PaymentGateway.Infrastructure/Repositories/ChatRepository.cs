using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Infrastructure.Interfaces;

namespace PaymentGateway.Infrastructure.Repositories;

public class ChatRepository(AppDbContext context)
    : RepositoryBase<ChatMessageEntity>(context), IChatRepository
{
    public async Task<List<ChatMessageEntity>> GetAllChatMessages()
    {
        return await GetSet()
            .Include(m => m.User)
            .OrderBy(m => m.Id)
            .ToListAsync();
    }

    public Task<ChatMessageEntity> AddChatMessage(ChatMessageEntity message)
    {
        GetSet().Add(message);
        return Task.FromResult(message);
    }
}