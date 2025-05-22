using AutoMapper;
using PaymentGateway.Core.Entities;
using PaymentGateway.Shared.DTOs.Device;

namespace PaymentGateway.Application.Mappings;

public class DeviceProfile : Profile
{
    public DeviceProfile()
    {
        CreateMap<DeviceEntity, DeviceDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.DeviceName))
            .ForMember(dest => dest.BindingAt, opt => opt.MapFrom(src => src.BindingAt));
    }
}