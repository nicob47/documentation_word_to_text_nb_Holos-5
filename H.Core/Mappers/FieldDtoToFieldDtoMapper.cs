using H.Core.Factories.Fields;

namespace H.Core.Mappers;

public class FieldDtoToFieldDtoMapper : IModelMapper<FieldSystemComponentDto, FieldSystemComponentDto>
{
    public FieldSystemComponentDto Map(FieldSystemComponentDto source)
        => PropertyMapper.Map<FieldSystemComponentDto, FieldSystemComponentDto>(source);
}
