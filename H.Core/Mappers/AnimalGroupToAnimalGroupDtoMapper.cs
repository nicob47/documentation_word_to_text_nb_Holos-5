using H.Core.Factories.Animals;
using H.Core.Models.Animals;

namespace H.Core.Mappers;

/// <summary>
/// Maps AnimalGroup domain model to AnimalGroupDto.
/// The reverse mapping (DTO to domain) is handled by AnimalGroupDtoToAnimalGroupMapper.
/// </summary>
public class AnimalGroupToAnimalGroupDtoMapper : IModelMapper<AnimalGroup, AnimalGroupDto>
{
    public AnimalGroupDto Map(AnimalGroup source)
        => PropertyMapper.Map<AnimalGroup, AnimalGroupDto>(source);
}
