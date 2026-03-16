using H.Core.Factories.Animals;

namespace H.Core.Mappers;

public class AnimalGroupDtoToAnimalGroupDtoMapper : IModelMapper<AnimalGroupDto, AnimalGroupDto>
{
    public AnimalGroupDto Map(AnimalGroupDto source)
        => PropertyMapper.Map<AnimalGroupDto, AnimalGroupDto>(source);
}
