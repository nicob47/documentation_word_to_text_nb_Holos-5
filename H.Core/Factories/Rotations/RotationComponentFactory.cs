using H.Core.Mappers;
using H.Core.Models;

namespace H.Core.Factories.Rotations;

public class RotationComponentFactory : IFactory<RotationComponentDto>
{
    public RotationComponentDto CreateDto()
    {
        return new RotationComponentDto();
    }

    public RotationComponentDto CreateDto(Farm farm)
    {
        return new RotationComponentDto();
    }

    public IDto CreateDtoFromDtoTemplate(IDto template)
    {
        if (template is not RotationComponentDto rotationTemplate)
        {
            throw new ArgumentException($"Template must be of type {nameof(RotationComponentDto)}", nameof(template));
        }

        var dto = new RotationComponentDto();
        PropertyMapper.CopyTo(rotationTemplate, dto);

        return dto;
    }
}
