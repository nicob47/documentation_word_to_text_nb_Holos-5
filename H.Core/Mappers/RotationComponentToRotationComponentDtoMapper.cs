using H.Core.Factories.Rotations;
using H.Core.Models.LandManagement.Rotation;

namespace H.Core.Mappers;

public class RotationComponentToRotationComponentDtoMapper : IModelMapper<RotationComponent, RotationComponentDto>
{
    public RotationComponentDto Map(RotationComponent source)
    {
        var dest = PropertyMapper.Map<RotationComponent, RotationComponentDto>(source);
        dest.FieldArea = source.FieldSystemComponent.FieldArea;
        return dest;
    }
}
