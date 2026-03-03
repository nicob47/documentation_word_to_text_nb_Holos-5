using AutoMapper;
using H.Core.Factories.Rotations;
using H.Core.Models.LandManagement.Rotation;

namespace H.Core.Mappers;

public class RotationComponentToRotationComponentDtoMapper : Profile
{
    public RotationComponentToRotationComponentDtoMapper()
    {
        CreateMap<RotationComponent, RotationComponentDto>()
            .ForMember(dest => dest.FieldArea, opt => opt.MapFrom(src => src.FieldSystemComponent.FieldArea));
    }
}
