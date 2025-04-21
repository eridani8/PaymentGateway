using PaymentGateway.Shared.DTOs.Chat;

namespace PaymentGateway.Core.Interfaces;

public interface IChatMessageService
{
    Task<List<ChatMessageDto>> GetAllMessages();
    Task<ChatMessageDto> SaveMessage(ChatMessageDto message);
} 