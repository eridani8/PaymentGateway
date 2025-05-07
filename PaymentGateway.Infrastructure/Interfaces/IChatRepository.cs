using PaymentGateway.Core.Entities;

namespace PaymentGateway.Infrastructure.Interfaces;

public interface IChatRepository
{
    Task<List<ChatMessageEntity>> GetAllChatMessages();
    Task<ChatMessageEntity> AddChatMessage(ChatMessageEntity message);
} 