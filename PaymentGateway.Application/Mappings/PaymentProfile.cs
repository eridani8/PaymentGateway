using AutoMapper;
using PaymentGateway.Application.DTOs.Payment;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Application.Mappings;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<PaymentEntity, PaymentResponseDto>();
        CreateMap<PaymentCreateDto, PaymentEntity>();
    }
}