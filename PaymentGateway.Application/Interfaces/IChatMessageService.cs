using PaymentGateway.Shared.DTOs.Chat;

namespace PaymentGateway.Application.Interfaces;

public interface IChatMessageService
{
    Task<List<ChatMessageDto>> GetAllChatMessages();
    Task<ChatMessageDto> SaveChatMessage(ChatMessageDto message);
} 