using H.Core.Mappers;
using H.Core.Models.Climate;
using H.Core.Providers.Climate;

namespace H.Core.Test.Mappers;

[TestClass]
public class ClimateMapperTests
{
    [TestMethod]
    public void DailyClimateDataToDto_MapsPETToEvapotranspiration()
    {
        var mapper = new DailyClimateDataToDailyClimateDtoMapper();
        var source = new DailyClimateData
        {
            Year = 2024,
            MeanDailyPET = 3.5,
            MeanDailyPrecipitation = 2.1,
            MeanDailyAirTemperature = 15.0
        };

        var result = mapper.Map(source);

        Assert.AreEqual(2024, result.Year);
        Assert.AreEqual(3.5, result.MeanDailyEvapotranspiration);
        Assert.AreEqual(2.1, result.MeanDailyPrecipitation);
    }

    [TestMethod]
    public void DailyClimateDtoToData_MapsTotalPETAndPPT()
    {
        var mapper = new DailyClimateDtoToDailyClimateDataMapper();
        var source = new DailyClimateDto
        {
            Year = 2023,
            TotalPET = 4.2,
            TotalPPT = 1.8
        };

        var result = mapper.Map(source);

        Assert.AreEqual(2023, result.Year);
        Assert.AreEqual(4.2, result.MeanDailyPET);
        Assert.AreEqual(1.8, result.MeanDailyPrecipitation);
    }

    [TestMethod]
    public void DailyClimateDtoToDto_ClonesProperties()
    {
        var mapper = new DailyClimateDtoToDailyClimateDtoMapper();
        var source = new DailyClimateDto
        {
            Year = 2025,
            TotalPET = 5.0,
            TotalPPT = 3.0,
            MeanDailyPrecipitation = 2.5
        };

        var result = mapper.Map(source);

        Assert.AreNotSame(source, result);
        Assert.AreEqual(2025, result.Year);
        Assert.AreEqual(5.0, result.TotalPET);
        Assert.AreEqual(3.0, result.TotalPPT);
        Assert.AreEqual(2.5, result.MeanDailyPrecipitation);
    }
}
