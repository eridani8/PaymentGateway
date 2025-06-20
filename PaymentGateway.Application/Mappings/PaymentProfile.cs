﻿using AutoMapper;
using PaymentGateway.Application.Mappings.Converters;
using PaymentGateway.Core.Entities;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Application.Mappings;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<PaymentEntity, PaymentDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.RequisiteId, opt => opt.MapFrom(src => src.RequisiteId))
            .ForMember(dest => dest.Requisite, opt => opt.MapFrom(src => src.Requisite))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.TransactionId))
            .ForMember(dest => dest.ManualConfirmUserId, opt => opt.MapFrom(src => src.ManualConfirmUserId))
            .ForMember(dest => dest.CanceledByUserId, opt => opt.MapFrom(src => src.CanceledByUserId))
            
            .ForMember(dest => dest.CreatedAt, opt => opt.ConvertUsing(new UtcToLocalDateTimeConverter(), src => src.CreatedAt))
            .ForMember(dest => dest.ProcessedAt, opt => opt.ConvertUsing(new UtcToLocalNullableDateTimeConverter(), src => src.ProcessedAt))
            .ForMember(dest => dest.ExpiresAt, opt => opt.ConvertUsing(new UtcToLocalNullableDateTimeConverter(), src => src.ExpiresAt));
        
        CreateMap<PaymentCreateDto, PaymentEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.CreateVersion7()))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => PaymentStatus.Created))
            .ForMember(dest => dest.ExpiresAt, opt => opt.MapFrom(src => DateTime.UtcNow.AddMinutes(5))) // ~
            .ForMember(dest => dest.RequisiteId, opt => opt.MapFrom(src => (Guid?)null))
            .ForMember(dest => dest.Requisite, opt => opt.MapFrom(src => (RequisiteEntity?)null))
            .ForMember(dest => dest.ProcessedAt, opt => opt.MapFrom(src => (DateTime?)null))
            .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => (Guid?)null))
            .ForMember(dest => dest.Transaction, opt => opt.MapFrom(src => (TransactionEntity?)null));
    }
}