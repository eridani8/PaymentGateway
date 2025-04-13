using AutoMapper;
using PaymentGateway.Application.Mappings.Converters;
using PaymentGateway.Core.Entities;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Application.Mappings;

public class RequisiteProfile : Profile
{
    public RequisiteProfile()
    {
        CreateMap<RequisiteEntity, RequisiteDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.PaymentType, opt => opt.MapFrom(src => src.PaymentType))
            .ForMember(dest => dest.PaymentData, opt => opt.MapFrom(src => src.PaymentData))
            .ForMember(dest => dest.BankNumber, opt => opt.MapFrom(src => src.BankNumber))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.PaymentId, opt => opt.MapFrom(src => src.PaymentId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.LastOperationTime, opt => opt.ConvertUsing(new UtcToLocalNullableDateTimeConverter(), src => src.LastOperationTime))
            .ForMember(dest => dest.ReceivedFunds, opt => opt.MapFrom(src => src.ReceivedFunds))
            .ForMember(dest => dest.MaxAmount, opt => opt.MapFrom(src => src.MaxAmount))
            .ForMember(dest => dest.Cooldown, opt => opt.MapFrom(src => src.Cooldown))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
            .ForMember(dest => dest.WorkFrom, opt => opt.MapFrom(src => src.WorkFrom))
            .ForMember(dest => dest.WorkTo, opt => opt.MapFrom(src => src.WorkTo))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Status == RequisiteStatus.Active))
            
            .ForMember(dest => dest.CreatedAt, opt => opt.ConvertUsing(new UtcToLocalDateTimeConverter(), src => src.CreatedAt))
            .ForMember(dest => dest.WorkFrom, opt => opt.ConvertUsing(new UtcToLocalTimeOnlyConverter(), src => src.WorkFrom))
            .ForMember(dest => dest.WorkTo, opt => opt.ConvertUsing(new UtcToLocalTimeOnlyConverter(), src => src.WorkTo))
            .ForMember(dest => dest.LastFundsResetAt, opt => opt.ConvertUsing(new UtcToLocalNullableDateTimeConverter(), src => src.LastFundsResetAt));


        CreateMap<RequisiteCreateDto, RequisiteEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.CreateVersion7()))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.PaymentType, opt => opt.MapFrom(src => src.PaymentType))
            .ForMember(dest => dest.PaymentData, opt => opt.MapFrom(src => src.PaymentData))
            .ForMember(dest => dest.BankNumber, opt => opt.MapFrom(src => src.BankNumber))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive ? RequisiteStatus.Active : RequisiteStatus.Inactive))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.MaxAmount, opt => opt.MapFrom(src => src.MaxAmount))
            .ForMember(dest => dest.Cooldown, opt => opt.MapFrom(src => src.Cooldown))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
            .ForMember(dest => dest.LastFundsResetAt, opt => opt.MapFrom(src => DateTime.UtcNow.Date))
            
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.WorkFrom, opt => opt.ConvertUsing(new LocalToUtcTimeOnlyConverter(), src => src.WorkFrom))
            .ForMember(dest => dest.WorkTo, opt => opt.ConvertUsing(new LocalToUtcTimeOnlyConverter(), src => src.WorkTo))
            
            .ForMember(dest => dest.LastOperationTime, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentId, opt => opt.Ignore())
            .ForMember(dest => dest.Payment, opt => opt.Ignore())
            .ForMember(dest => dest.ReceivedFunds, opt => opt.MapFrom(src => 0m));


        CreateMap<RequisiteUpdateDto, RequisiteEntity>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.PaymentType, opt => opt.MapFrom(src => src.PaymentType))
            .ForMember(dest => dest.PaymentData, opt => opt.MapFrom(src => src.PaymentData))
            .ForMember(dest => dest.BankNumber, opt => opt.MapFrom(src => src.BankNumber))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive ? RequisiteStatus.Active : RequisiteStatus.Inactive))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.MaxAmount, opt => opt.MapFrom(src => src.MaxAmount))
            .ForMember(dest => dest.Cooldown, opt => opt.MapFrom(src => src.Cooldown))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
            
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.WorkFrom, opt => opt.ConvertUsing(new LocalToUtcTimeOnlyConverter(), src => src.WorkFrom))
            .ForMember(dest => dest.WorkTo, opt => opt.ConvertUsing(new LocalToUtcTimeOnlyConverter(), src => src.WorkTo))
            
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.LastOperationTime, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentId, opt => opt.Ignore())
            .ForMember(dest => dest.Payment, opt => opt.Ignore())
            .ForMember(dest => dest.ReceivedFunds, opt => opt.Ignore())
            .ForMember(dest => dest.LastFundsResetAt, opt => opt.Ignore());
    }
}