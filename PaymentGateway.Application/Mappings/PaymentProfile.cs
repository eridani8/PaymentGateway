using AutoMapper;
using PaymentGateway.Application.DTOs.Payment;
using PaymentGateway.Application.Mappings.Converters;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Application.Mappings;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<PaymentEntity, PaymentResponseDto>()
            .ForMember(dest => dest.CreatedAt,
                opt =>
                    opt.ConvertUsing(new UtcToLocalDateTimeConverter(), src => src.CreatedAt))
            .ForMember(dest => dest.ProcessedAt, 
                opt => 
                    opt.ConvertUsing(new UtcToLocalNullableDateTimeConverter(), src => src.ProcessedAt))
            .ForMember(dest => dest.ExpiresAt, 
                opt => 
                    opt.ConvertUsing(new UtcToLocalNullableDateTimeConverter(), src => src.ExpiresAt));
        
        CreateMap<PaymentCreateDto, PaymentEntity>();
    }
}