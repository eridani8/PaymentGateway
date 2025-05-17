using AutoMapper;
using PaymentGateway.Shared.DTOs.Device;

namespace PaymentGateway.Application.Mappings;

public class DeviceProfile : Profile
{
    public DeviceProfile()
    {
        CreateMap<PingDto, DeviceDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model))
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => DateTime.Now));
    }
}