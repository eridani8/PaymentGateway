using AutoMapper;
using PaymentGateway.Core.Entities;
using PaymentGateway.Shared.DTOs.Chat;

namespace PaymentGateway.Application.Mappings;

public class ChatMessageProfile : Profile
{
    public ChatMessageProfile()
    {
        CreateMap<ChatMessageEntity, ChatMessageDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty));
        
        CreateMap<ChatMessageDto, ChatMessageEntity>();
    }
} 