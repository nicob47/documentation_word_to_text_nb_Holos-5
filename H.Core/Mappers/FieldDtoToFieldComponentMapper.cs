using H.Core.Factories.Fields;
using H.Core.Models.LandManagement.Fields;

namespace H.Core.Mappers;

public class FieldDtoToFieldComponentMapper : IModelMapper<FieldSystemComponentDto, FieldSystemComponent>
{
    public FieldSystemComponent Map(FieldSystemComponentDto source)
        => PropertyMapper.Map<FieldSystemComponentDto, FieldSystemComponent>(source);
}
