using AutoMapper;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared.DTOs.Chat;

namespace PaymentGateway.Application.Services;

public class ChatMessageService(
    IUnitOfWork unitOfWork, 
    IMapper mapper) : IChatMessageService
{
    public async Task<List<ChatMessageDto>> GetAllMessages()
    {
        var messages = await unitOfWork.ChatMessageRepository.GetAll();
        return mapper.Map<List<ChatMessageDto>>(messages);
    }

    public async Task<ChatMessageDto> SaveMessage(ChatMessageDto message)
    {
        var entity = mapper.Map<ChatMessageEntity>(message);
        
        var savedMessage = await unitOfWork.ChatMessageRepository.Add(entity);
        return mapper.Map<ChatMessageDto>(savedMessage);
    }
} 