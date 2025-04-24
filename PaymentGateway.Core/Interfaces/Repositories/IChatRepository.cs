using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Interfaces.Repositories;

public interface IChatRepository
{
    Task<List<ChatMessageEntity>> GetAllChatMessages();
    Task<ChatMessageEntity> AddChatMessage(ChatMessageEntity message);
} 