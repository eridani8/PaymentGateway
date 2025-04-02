using AutoMapper;
using PaymentGateway.Application.DTOs;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Application.Mappings;

public class RequisiteProfile : Profile
{
    public RequisiteProfile()
    {
        CreateMap<RequisiteEntity, RequisiteResponseDto>();
        CreateMap<RequisiteCreateDto, RequisiteEntity>();
        CreateMap<RequisiteUpdateDto, RequisiteEntity>()
            .ForAllMembers(o => 
                o.Condition((dto, entity, srcMember) 
                    => srcMember != null));
    }
}