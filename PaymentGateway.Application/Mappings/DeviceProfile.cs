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
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.DeviceName))
            .ForMember(dest => dest.Fingerprint, opt => opt.MapFrom(src => src.Fingerprint))
            .ForMember(dest => dest.BindingAt, opt => opt.MapFrom(src => src.BindingAt))
            .ForMember(dest => dest.RequisiteId, opt => opt.MapFrom(src => src.RequisiteId));

        CreateMap<DeviceDto, DeviceEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.DeviceName))
            .ForMember(dest => dest.Fingerprint, opt => opt.MapFrom(src => src.Fingerprint))
            .ForMember(dest => dest.BindingAt, opt => opt.MapFrom(src => src.BindingAt))
            .ForMember(dest => dest.RequisiteId, opt => opt.MapFrom(src => src.RequisiteId));
    }
}