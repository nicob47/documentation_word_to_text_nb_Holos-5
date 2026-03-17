using H.Core.Factories.Animals;

namespace H.Core.Mappers;

public class AnimalComponentDtoToAnimalComponentDtoMapper : IModelMapper<AnimalComponentDto, AnimalComponentDto>
{
    public AnimalComponentDto Map(AnimalComponentDto source)
        => PropertyMapper.Map<AnimalComponentDto, AnimalComponentDto>(source);
}
