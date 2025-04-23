using AutoMapper;
using PaymentGateway.Application.Mappings.Converters;
using PaymentGateway.Core.Entities;
using PaymentGateway.Shared.DTOs.Chat;

namespace PaymentGateway.Application.Mappings;

public class ChatMessageProfile : Profile
{
    public ChatMessageProfile()
    {
        CreateMap<ChatMessageEntity, ChatMessageDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
            .ForMember(dest => dest.Timestamp, opt => opt.ConvertUsing(new UtcToLocalDateTimeConverter(), src => src.Timestamp));
        
        CreateMap<ChatMessageDto, ChatMessageEntity>();
    }
} 