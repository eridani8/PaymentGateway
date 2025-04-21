using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Interfaces.Repositories;

public interface IChatMessageRepository
{
    Task<List<ChatMessageEntity>> GetAll();
    Task<ChatMessageEntity> Add(ChatMessageEntity message);
} 