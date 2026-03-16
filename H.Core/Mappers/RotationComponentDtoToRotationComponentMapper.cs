using H.Core.Factories.Rotations;
using H.Core.Models.LandManagement.Rotation;

namespace H.Core.Mappers;

public class RotationComponentDtoToRotationComponentMapper : IModelMapper<RotationComponentDto, RotationComponent>
{
    public RotationComponent Map(RotationComponentDto source)
        => PropertyMapper.Map<RotationComponentDto, RotationComponent>(source);
}
