﻿using AutoMapper;
using PaymentGateway.Application.Mappings.Converters;
using PaymentGateway.Core.Entities;
using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Application.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserEntity, UserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.RequisitesCount, opt => opt.MapFrom(src => src.RequisitesCount))
            .ForMember(dest => dest.MaxRequisitesCount, opt => opt.MapFrom(src => src.MaxRequisitesCount))
            .ForMember(dest => dest.MaxDailyMoneyReceptionLimit, opt => opt.MapFrom(src => src.MaxDailyMoneyReceptionLimit))
            .ForMember(dest => dest.ReceivedDailyFunds, opt => opt.MapFrom(src => src.ReceivedDailyFunds))
            .ForMember(dest => dest.LastFundsResetAt, opt => opt.MapFrom(src => src.LastFundsResetAt))
            .ForMember(dest => dest.CreatedAt, opt => opt.ConvertUsing(new UtcToLocalDateTimeConverter(), src => src.CreatedAt))
            .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.Balance))
            .ForMember(dest => dest.Frozen, opt => opt.MapFrom(src => src.Frozen))
            .ForMember(dest => dest.Profit, opt => opt.MapFrom(src => src.Profit));

        CreateMap<UserEntity, WalletDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.Balance))
            .ForMember(dest => dest.Frozen, opt => opt.MapFrom(src => src.Frozen))
            .ForMember(dest => dest.Profit, opt => opt.MapFrom(src => src.Profit));
        
        CreateMap<CreateUserDto, UserEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.CreateVersion7()))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.MaxRequisitesCount, opt => opt.MapFrom(src => src.MaxRequisitesCount))
            .ForMember(dest => dest.MaxDailyMoneyReceptionLimit, opt => opt.MapFrom(src => src.MaxDailyMoneyReceptionLimit))
            .ForMember(dest => dest.ReceivedDailyFunds, opt => opt.MapFrom(src => 0m))
            .ForMember(dest => dest.LastFundsResetAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.Frozen, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.Profit, opt => opt.MapFrom(src => 0));

        CreateMap<UpdateUserDto, UserEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.MaxRequisitesCount, opt => opt.MapFrom(src => src.MaxRequisitesCount))
            .ForMember(dest => dest.MaxDailyMoneyReceptionLimit, opt => opt.MapFrom(src => src.MaxDailyMoneyReceptionLimit))
            .ForMember(dest => dest.UserName, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
            .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
            .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore());
    }   
}