using AutoMapper;
using PaymentGateway.Application.Mappings.Converters;
using PaymentGateway.Core.Entities;
using PaymentGateway.Shared.DTOs.Transaction;

namespace PaymentGateway.Application.Mappings;

public class TransactionProfile : Profile
{
    public TransactionProfile()
    {
        CreateMap<TransactionEntity, TransactionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.RequisiteId, opt => opt.MapFrom(src => src.RequisiteId))
            .ForMember(dest => dest.PaymentId, opt => opt.MapFrom(src => src.PaymentId))
            .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source))
            .ForMember(dest => dest.ExtractedAmount, opt => opt.MapFrom(src => src.ExtractedAmount))
            .ForMember(dest => dest.RawMessage, opt => opt.MapFrom(src => src.RawMessage))
            .ForMember(dest => dest.ReceivedAt, opt => opt.ConvertUsing(new UtcToLocalDateTimeConverter(), src => src.ReceivedAt));
        
        CreateMap<TransactionCreateDto, TransactionEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.CreateVersion7()))
            .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source))
            .ForMember(dest => dest.ExtractedAmount, opt => opt.MapFrom(src => src.ExtractedAmount))
            
            .ForMember(dest => dest.RawMessage, opt => opt.MapFrom(src => src.RawMessage))
            .ForMember(dest => dest.ReceivedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            
            .ForMember(dest => dest.RequisiteId, opt => opt.Ignore())
            .ForMember(dest => dest.Requisite, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentId, opt => opt.Ignore())
            .ForMember(dest => dest.Payment, opt => opt.Ignore());
    }
}