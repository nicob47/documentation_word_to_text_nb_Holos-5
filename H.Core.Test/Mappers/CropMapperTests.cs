using H.Core.Enumerations;
using H.Core.Factories.Crops;
using H.Core.Mappers;
using H.Core.Models.LandManagement.Fields;

namespace H.Core.Test.Mappers;

[TestClass]
public class CropMapperTests
{
    [TestMethod]
    public void CropDtoToCropViewItemMapper_MapsWetYieldToYield()
    {
        var mapper = new CropDtoToCropViewItemMapper();
        var source = new CropDto
        {
            CropType = CropType.Barley,
            Year = 2024,
            WetYield = 4500.0,
            AmountOfIrrigation = 120.0
        };

        var result = mapper.Map(source);

        Assert.AreEqual(CropType.Barley, result.CropType);
        Assert.AreEqual(2024, result.Year);
        Assert.AreEqual(4500.0, result.Yield);
        Assert.AreEqual(120.0, result.AmountOfIrrigation);
    }

    [TestMethod]
    public void CropViewItemToCropDtoMapper_MapsYieldToWetYield()
    {
        var mapper = new CropViewItemToCropDtoMapper();
        var source = new CropViewItem
        {
            CropType = CropType.Wheat,
            Year = 2023,
            Yield = 3200.0,
            AmountOfIrrigation = 80.0
        };

        var result = mapper.Map(source);

        Assert.AreEqual(CropType.Wheat, result.CropType);
        Assert.AreEqual(2023, result.Year);
        Assert.AreEqual(3200.0, result.WetYield);
        Assert.AreEqual(80.0, result.AmountOfIrrigation);
    }

    [TestMethod]
    public void CropDtoToCropDtoMapper_ClonesProperties()
    {
        var mapper = new CropDtoToCropDtoMapper();
        var source = new CropDto
        {
            CropType = CropType.Oats,
            Year = 2025,
            WetYield = 2800.0,
            HerbicideUsed = true
        };

        var result = mapper.Map(source);

        Assert.AreNotSame(source, result);
        Assert.AreEqual(CropType.Oats, result.CropType);
        Assert.AreEqual(2025, result.Year);
        Assert.AreEqual(2800.0, result.WetYield);
        Assert.IsTrue(result.HerbicideUsed);
    }
}
