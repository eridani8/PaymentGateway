using PaymentGateway.Shared.DTOs.Chat;

namespace PaymentGateway.Core.Interfaces;

public interface IChatMessageService
{
    Task<List<ChatMessageDto>> GetAllChatMessages();
    Task<ChatMessageDto> SaveChatMessage(ChatMessageDto message);
} 