using AutoMapper;
using PaymentGateway.Application.DTOs;
using PaymentGateway.Application.DTOs.Requisite;
using PaymentGateway.Application.Mappings.Converters;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Enums;

namespace PaymentGateway.Application.Mappings;

public class RequisiteProfile : Profile
{
    public RequisiteProfile()
    {
        CreateMap<RequisiteEntity, RequisiteResponseDto>()
            .ForMember(dest => dest.CreatedAt,
                opt =>
                    opt.ConvertUsing(new UtcToLocalDateTimeConverter(), src => src.CreatedAt))
            .ForMember(dest => dest.WorkFrom,
                opt =>
                    opt.ConvertUsing(new UtcToLocalTimeOnlyConverter(), src => src.WorkFrom))
            .ForMember(dest => dest.WorkTo,
                opt =>
                    opt.ConvertUsing(new UtcToLocalTimeOnlyConverter(), src => src.WorkTo));

        CreateMap<RequisiteCreateDto, RequisiteEntity>()
            .ForMember(dest => dest.CreatedAt,
                opt =>
                    opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.WorkFrom,
                opt =>
                    opt.ConvertUsing(new LocalToUtcTimeOnlyConverter(), src => src.WorkFrom))
            .ForMember(dest => dest.WorkTo,
                opt =>
                    opt.ConvertUsing(new LocalToUtcTimeOnlyConverter(), src => src.WorkTo));

        CreateMap<RequisiteUpdateDto, RequisiteEntity>()
            .ForMember(dest => dest.CreatedAt,
                opt =>
                    opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.WorkFrom,
                opt =>
                    opt.ConvertUsing(new LocalToUtcTimeOnlyConverter(), src => src.WorkFrom))
            .ForMember(dest => dest.WorkTo,
                opt =>
                    opt.ConvertUsing(new LocalToUtcTimeOnlyConverter(), src => src.WorkTo));
    }
}