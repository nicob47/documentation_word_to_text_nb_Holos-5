using H.Core.Models.Climate;
using H.Core.Providers.Climate;

namespace H.Core.Mappers;

public class DailyClimateDataToDailyClimateDtoMapper : IModelMapper<DailyClimateData, DailyClimateDto>
{
    public DailyClimateDto Map(DailyClimateData source)
    {
        var dest = PropertyMapper.Map<DailyClimateData, DailyClimateDto>(source);
        dest.MeanDailyEvapotranspiration = source.MeanDailyPET;
        return dest;
    }
}
