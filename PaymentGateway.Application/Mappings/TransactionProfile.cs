using AutoMapper;
using PaymentGateway.Application.DTOs.Transaction;
using PaymentGateway.Application.Mappings.Converters;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Application.Mappings;

public class TransactionProfile : Profile
{
    public TransactionProfile()
    {
        CreateMap<TransactionEntity, TransactionResponseDto>()
            .ForMember(dest => dest.ReceivedAt,
                opt =>
                    opt.ConvertUsing(new UtcToLocalDateTimeConverter(), src => src.ReceivedAt));
        
        CreateMap<TransactionCreateDto, TransactionEntity>()
            .ForMember(dest => dest.ReceivedAt,
                opt =>
                    opt.MapFrom(src => DateTime.UtcNow));
    }
}