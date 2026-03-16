using H.Core.Models.Climate;
using H.Core.Providers.Climate;

namespace H.Core.Mappers;

public class DailyClimateDtoToDailyClimateDataMapper : IModelMapper<DailyClimateDto, DailyClimateData>
{
    public DailyClimateData Map(DailyClimateDto source)
    {
        var dest = PropertyMapper.Map<DailyClimateDto, DailyClimateData>(source);
        dest.MeanDailyPET = source.TotalPET;
        dest.MeanDailyPrecipitation = source.TotalPPT;
        return dest;
    }
}
