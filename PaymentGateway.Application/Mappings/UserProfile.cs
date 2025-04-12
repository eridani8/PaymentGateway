using AutoMapper;
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
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles));

        CreateMap<CreateUserDto, UserEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.CreateVersion7()))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles));
    }   
}