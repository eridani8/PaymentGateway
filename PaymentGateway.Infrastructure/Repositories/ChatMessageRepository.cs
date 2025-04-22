using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Core.Interfaces.Repositories;
using PaymentGateway.Infrastructure.Data;

namespace PaymentGateway.Infrastructure.Repositories;

public class ChatMessageRepository(AppDbContext context, ICache cache)
    : RepositoryBase<ChatMessageEntity>(context, cache), IChatMessageRepository
{
    private readonly AppDbContext _context = context;

    public new async Task<List<ChatMessageEntity>> GetAll()
    {
        return await _context.ChatMessages
            .Include(m => m.User)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    public new async Task<ChatMessageEntity> Add(ChatMessageEntity message)
    {
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }
}