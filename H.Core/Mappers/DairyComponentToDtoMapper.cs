using H.Core.Factories.Animals.Dairy;
using H.Core.Models.Animals.Dairy;

namespace H.Core.Mappers;

/// <summary>
/// Maps between DairyComponent domain model and DairyComponentDto.
/// PropertyMapper handles base class properties (from AnimalComponentBase/AnimalComponentDto) automatically.
/// </summary>
public class DairyComponentToDtoMapper : IModelMapper<DairyComponent, DairyComponentDto>
{
    public DairyComponentDto Map(DairyComponent source)
        => PropertyMapper.Map<DairyComponent, DairyComponentDto>(source);
}

public class DairyComponentDtoToComponentMapper : IModelMapper<DairyComponentDto, DairyComponent>
{
    public DairyComponent Map(DairyComponentDto source)
        => PropertyMapper.Map<DairyComponentDto, DairyComponent>(source);
}
