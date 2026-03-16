using H.Core.Factories.Animals;
using H.Core.Models.Animals;

namespace H.Core.Mappers;

public class AnimalComponentBaseToAnimalComponentDtoMapper : IModelMapper<AnimalComponentBase, AnimalComponentDto>
{
    public AnimalComponentDto Map(AnimalComponentBase source)
        => PropertyMapper.Map<AnimalComponentBase, AnimalComponentDto>(source);
}
