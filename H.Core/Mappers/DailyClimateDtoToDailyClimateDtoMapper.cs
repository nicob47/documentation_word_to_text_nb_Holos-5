using H.Core.Models.Climate;

namespace H.Core.Mappers;

public class DailyClimateDtoToDailyClimateDtoMapper : IModelMapper<DailyClimateDto, DailyClimateDto>
{
    public DailyClimateDto Map(DailyClimateDto source)
        => PropertyMapper.Map<DailyClimateDto, DailyClimateDto>(source);
}
