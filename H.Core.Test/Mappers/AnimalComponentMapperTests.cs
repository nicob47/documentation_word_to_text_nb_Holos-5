using H.Core.Factories.Animals;
using H.Core.Factories.Animals.Dairy;
using H.Core.Mappers;
using H.Core.Models.Animals.Dairy;

namespace H.Core.Test.Mappers;

[TestClass]
public class AnimalComponentMapperTests
{
    [TestMethod]
    public void AnimalComponentBaseToDto_MapsProperties()
    {
        var mapper = new AnimalComponentBaseToAnimalComponentDtoMapper();
        var source = new DairyComponent
        {
            Name = "Dairy Herd A"
        };

        var result = mapper.Map(source);

        Assert.AreEqual("Dairy Herd A", result.Name);
    }

    [TestMethod]
    public void AnimalComponentDtoToDto_ClonesProperties()
    {
        var mapper = new AnimalComponentDtoToAnimalComponentDtoMapper();
        var source = new AnimalComponentDto
        {
            Name = "Clone Source"
        };

        var result = mapper.Map(source);

        Assert.AreNotSame(source, result);
        Assert.AreEqual("Clone Source", result.Name);
    }

    [TestMethod]
    public void AnimalComponentDtoToModel_ThrowsForAbstractType()
    {
        var mapper = new AnimalComponentDtoToAnimalComponentMapper();
        var source = new AnimalComponentDto { Name = "Test" };
        Assert.ThrowsExactly<NotSupportedException>(() => mapper.Map(source));
    }

    [TestMethod]
    public void DairyComponentToDtoMapper_MapsProperties()
    {
        var mapper = new DairyComponentToDtoMapper();
        var source = new DairyComponent
        {
            Name = "Dairy Herd B"
        };

        var result = mapper.Map(source);

        Assert.AreEqual("Dairy Herd B", result.Name);
    }

    [TestMethod]
    public void DairyComponentDtoToComponentMapper_MapsProperties()
    {
        var mapper = new DairyComponentDtoToComponentMapper();
        var source = new DairyComponentDto
        {
            Name = "From DTO"
        };

        var result = mapper.Map(source);

        Assert.IsNotNull(result);
        Assert.AreEqual("From DTO", result.Name);
    }
}
