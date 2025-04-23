using AutoMapper;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Interfaces;
using PaymentGateway.Shared.DTOs.Chat;

namespace PaymentGateway.Application.Services;

public class ChatMessageService(
    IUnitOfWork unitOfWork, 
    IMapper mapper) : IChatMessageService
{
    public async Task<List<ChatMessageDto>> GetAllChatMessages()
    {
        var messages = await unitOfWork.ChatMessageRepository.GetAllChatMessages();
        return mapper.Map<List<ChatMessageDto>>(messages);
    }

    public async Task<ChatMessageDto> SaveChatMessage(ChatMessageDto message)
    {
        var entity = mapper.Map<ChatMessageEntity>(message);
        
        var savedMessage = await unitOfWork.ChatMessageRepository.AddChatMessage(entity);
        return mapper.Map<ChatMessageDto>(savedMessage);
    }
} 