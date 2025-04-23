using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Interfaces.Repositories;

public interface IChatMessageRepository
{
    Task<List<ChatMessageEntity>> GetAllChatMessages();
    Task<ChatMessageEntity> AddChatMessage(ChatMessageEntity message);
} 