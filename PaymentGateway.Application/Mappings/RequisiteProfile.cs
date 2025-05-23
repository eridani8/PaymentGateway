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
            .ForMember(dest => dest.Payment, opt => opt.MapFrom(src => src.Payment))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.LastOperationTime, opt => opt.ConvertUsing(new UtcToLocalNullableDateTimeConverter(), src => src.LastOperationTime))
            .ForMember(dest => dest.DayReceivedFunds, opt => opt.MapFrom(src => src.DayReceivedFunds))
            .ForMember(dest => dest.DayLimit, opt => opt.MapFrom(src => src.DayLimit))
            .ForMember(dest => dest.MonthReceivedFunds, opt => opt.MapFrom(src => src.MonthReceivedFunds))
            .ForMember(dest => dest.MonthLimit, opt => opt.MapFrom(src => src.MonthLimit))
            .ForMember(dest => dest.Cooldown, opt => opt.MapFrom(src => src.Cooldown))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
            .ForMember(dest => dest.DayOperationsCount, opt => opt.MapFrom(src => src.DayOperationsCount))
            
            .ForMember(dest => dest.CreatedAt, opt => opt.ConvertUsing(new UtcToLocalDateTimeConverter(), src => src.CreatedAt))
            .ForMember(dest => dest.WorkFrom, opt => opt.ConvertUsing(new UtcToLocalTimeOnlyConverter(), src => src.WorkFrom))
            .ForMember(dest => dest.WorkTo, opt => opt.ConvertUsing(new UtcToLocalTimeOnlyConverter(), src => src.WorkTo))
            .ForMember(dest => dest.LastFundsResetAt, opt => opt.ConvertUsing(new UtcToLocalNullableDateTimeConverter(), src => src.LastDayFundsResetAt))
            .ForMember(dest => dest.LastMonthlyFundsResetAt, opt => opt.ConvertUsing(new UtcToLocalNullableDateTimeConverter(), src => src.LastMonthlyFundsResetAt));


        CreateMap<RequisiteCreateDto, RequisiteEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.CreateVersion7()))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom((_, _, _, context) => 
            {
                if (context.Items.TryGetValue("UserId", out var userId) && userId is Guid guid)
                {
                    return guid;
                }
                throw new InvalidOperationException("UserId is required for mapping RequisiteCreateDto to RequisiteEntity");
            }))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.PaymentType, opt => opt.MapFrom(src => src.PaymentType))
            .ForMember(dest => dest.PaymentData, opt => opt.MapFrom(src => src.PaymentData))
            .ForMember(dest => dest.BankNumber, opt => opt.MapFrom(src => src.BankNumber))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.DayLimit, opt => opt.MapFrom(src => src.MaxAmount))
            .ForMember(dest => dest.DayReceivedFunds, opt => opt.MapFrom(src => 0m))
            .ForMember(dest => dest.Cooldown, opt => opt.MapFrom(src => src.Cooldown))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
            .ForMember(dest => dest.LastDayFundsResetAt, opt => opt.MapFrom(src => DateTime.UtcNow.Date))
            .ForMember(dest => dest.MonthLimit, opt => opt.MapFrom(src => src.MonthLimit))
            .ForMember(dest => dest.LastMonthlyFundsResetAt, opt => opt.MapFrom(src => DateTime.UtcNow.Date))
            .ForMember(dest => dest.MonthReceivedFunds, opt => opt.MapFrom(src => 0m))
            .ForMember(dest => dest.DeviceId, opt => opt.MapFrom(src => src.DeviceId))
            
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.WorkFrom, opt => opt.ConvertUsing(new LocalToUtcTimeOnlyConverter(), src => src.WorkFrom))
            .ForMember(dest => dest.WorkTo, opt => opt.ConvertUsing(new LocalToUtcTimeOnlyConverter(), src => src.WorkTo))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => RequisiteStatus.Frozen))
            .ForMember(dest => dest.LastOperationTime, opt => opt.MapFrom(src => (DateTime?)null))
            .ForMember(dest => dest.PaymentId, opt => opt.MapFrom(src => (Guid?)null))
            .ForMember(dest => dest.Payment, opt => opt.MapFrom(src => (PaymentEntity?)null));


        CreateMap<RequisiteUpdateDto, RequisiteEntity>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.PaymentType, opt => opt.MapFrom(src => src.PaymentType))
            .ForMember(dest => dest.PaymentData, opt => opt.MapFrom(src => src.PaymentData))
            .ForMember(dest => dest.BankNumber, opt => opt.MapFrom(src => src.BankNumber))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.DayLimit, opt => opt.MapFrom(src => src.MaxAmount))
            .ForMember(dest => dest.MonthLimit, opt => opt.MapFrom(src => src.MonthLimit))
            .ForMember(dest => dest.Cooldown, opt => opt.MapFrom(src => src.Cooldown))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
            .ForMember(dest => dest.WorkFrom, opt => opt.ConvertUsing(new LocalToUtcTimeOnlyConverter(), src => src.WorkFrom))
            .ForMember(dest => dest.WorkTo, opt => opt.ConvertUsing(new LocalToUtcTimeOnlyConverter(), src => src.WorkTo))
            // TODO device
            
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.LastOperationTime, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentId, opt => opt.Ignore())
            .ForMember(dest => dest.Payment, opt => opt.Ignore())
            .ForMember(dest => dest.DayReceivedFunds, opt => opt.Ignore())
            .ForMember(dest => dest.LastDayFundsResetAt, opt => opt.Ignore())
            .ForMember(dest => dest.MonthReceivedFunds, opt => opt.Ignore())
            .ForMember(dest => dest.LastMonthlyFundsResetAt, opt => opt.Ignore());
    }
}