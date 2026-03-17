using H.Core.Factories.Fields;
using H.Core.Models.LandManagement.Fields;

namespace H.Core.Mappers;

public class FieldComponentToDtoMapper : IModelMapper<FieldSystemComponent, FieldSystemComponentDto>
{
    public FieldSystemComponentDto Map(FieldSystemComponent source)
        => PropertyMapper.Map<FieldSystemComponent, FieldSystemComponentDto>(source);
}
