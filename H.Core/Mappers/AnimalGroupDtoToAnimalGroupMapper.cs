using H.Core.Factories.Animals;
using H.Core.Models.Animals;

namespace H.Core.Mappers;

public class AnimalGroupDtoToAnimalGroupMapper : IModelMapper<AnimalGroupDto, AnimalGroup>
{
    public AnimalGroup Map(AnimalGroupDto source)
        => PropertyMapper.Map<AnimalGroupDto, AnimalGroup>(source);
}
