using AutoMapper;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Interfaces;
using PaymentGateway.Shared.DTOs.Chat;

namespace PaymentGateway.Application.Services;

public class ChatMessageService(
    IUnitOfWork unit, 
    IMapper mapper) : IChatMessageService
{
    public async Task<List<ChatMessageDto>> GetAllChatMessages()
    {
        var messages = await unit.ChatRepository.GetAllChatMessages();
        return mapper.Map<List<ChatMessageDto>>(messages);
    }

    public async Task<ChatMessageDto> SaveChatMessage(ChatMessageDto message)
    {
        var entity = mapper.Map<ChatMessageEntity>(message);
        
        var savedMessage = await unit.ChatRepository.AddChatMessage(entity);
        await unit.Commit();
        return mapper.Map<ChatMessageDto>(savedMessage);
    }
} 