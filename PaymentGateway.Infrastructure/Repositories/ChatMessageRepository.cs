using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Core.Interfaces.Repositories;
using PaymentGateway.Infrastructure.Data;

namespace PaymentGateway.Infrastructure.Repositories;

public class ChatMessageRepository(AppDbContext context, ICache cache) : RepositoryBase<ChatMessageEntity>(context, cache), IChatMessageRepository
{
    public async Task<List<ChatMessageEntity>> GetAll()
    {
        return await context.ChatMessages
            .Include(m => m.User)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<ChatMessageEntity> Add(ChatMessageEntity message)
    {
        context.ChatMessages.Add(message);
        await context.SaveChangesAsync();
        return message;
    }
} 