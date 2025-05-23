using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Infrastructure.Interfaces;

namespace PaymentGateway.Infrastructure.Repositories;

public class ChatRepository(AppDbContext context)
    : RepositoryBase<ChatMessageEntity>(context), IChatRepository
{
    private readonly AppDbContext _context = context;

    public async Task<List<ChatMessageEntity>> GetAllChatMessages()
    {
        return await _context.ChatMessages
            .Include(m => m.User)
            .OrderBy(m => m.Id)
            .ToListAsync();
    }

    public async Task<ChatMessageEntity> AddChatMessage(ChatMessageEntity message)
    {
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }
}