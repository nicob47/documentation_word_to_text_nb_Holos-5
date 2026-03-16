using H.Core.Factories.Fields;
using H.Core.Mappers;
using H.Core.Models.LandManagement.Fields;

namespace H.Core.Test.Mappers;

[TestClass]
public class FieldMapperTests
{
    [TestMethod]
    public void FieldComponentToDtoMapper_MapsAllProperties()
    {
        var mapper = new FieldComponentToDtoMapper();
        var source = new FieldSystemComponent
        {
            Name = "West Quarter",
            FieldArea = 64.5,
            StartYear = 2018,
            EndYear = 2025
        };

        var result = mapper.Map(source);

        Assert.AreEqual("West Quarter", result.Name);
        Assert.AreEqual(64.5, result.FieldArea);
        Assert.AreEqual(2018, result.StartYear);
        Assert.AreEqual(2025, result.EndYear);
    }

    [TestMethod]
    public void FieldDtoToFieldComponentMapper_MapsAllProperties()
    {
        var mapper = new FieldDtoToFieldComponentMapper();
        var source = new FieldSystemComponentDto
        {
            Name = "East Quarter",
            FieldArea = 32.0,
            StartYear = 2020,
            EndYear = 2024
        };

        var result = mapper.Map(source);

        Assert.AreEqual("East Quarter", result.Name);
        Assert.AreEqual(32.0, result.FieldArea);
        Assert.AreEqual(2020, result.StartYear);
        Assert.AreEqual(2024, result.EndYear);
    }

    [TestMethod]
    public void FieldDtoToFieldDtoMapper_ClonesAllProperties()
    {
        var mapper = new FieldDtoToFieldDtoMapper();
        var source = new FieldSystemComponentDto
        {
            Name = "Clone Source",
            FieldArea = 100.0,
            StartYear = 2019,
            EndYear = 2026
        };

        var result = mapper.Map(source);

        Assert.AreNotSame(source, result);
        Assert.AreEqual("Clone Source", result.Name);
        Assert.AreEqual(100.0, result.FieldArea);
        Assert.AreEqual(2019, result.StartYear);
        Assert.AreEqual(2026, result.EndYear);
    }
}
