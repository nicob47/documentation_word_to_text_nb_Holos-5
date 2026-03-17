using H.Core.Enumerations;
using H.Core.Factories.Animals;
using H.Core.Mappers;
using H.Core.Models.Animals;

namespace H.Core.Test.Mappers;

[TestClass]
public class AnimalGroupMapperTests
{
    [TestMethod]
    public void AnimalGroupToDto_MapsGroupTypeAndName()
    {
        var mapper = new AnimalGroupToAnimalGroupDtoMapper();
        var source = new AnimalGroup
        {
            Name = "Heifers",
            GroupType = AnimalType.DairyHeifers
        };

        var result = mapper.Map(source);

        Assert.AreEqual("Heifers", result.Name);
        Assert.AreEqual(AnimalType.DairyHeifers, result.GroupType);
    }

    [TestMethod]
    public void AnimalGroupDtoToModel_MapsNullableGroupType()
    {
        var mapper = new AnimalGroupDtoToAnimalGroupMapper();
        var source = new AnimalGroupDto
        {
            Name = "Calves",
            GroupType = AnimalType.DairyCalves
        };

        var result = mapper.Map(source);

        Assert.AreEqual("Calves", result.Name);
        Assert.AreEqual(AnimalType.DairyCalves, result.GroupType);
    }

    [TestMethod]
    public void AnimalGroupDtoToDto_ClonesProperties()
    {
        var mapper = new AnimalGroupDtoToAnimalGroupDtoMapper();
        var source = new AnimalGroupDto
        {
            Name = "Clone Source",
            GroupType = AnimalType.BeefCowLactating
        };

        var result = mapper.Map(source);

        Assert.AreNotSame(source, result);
        Assert.AreEqual("Clone Source", result.Name);
        Assert.AreEqual(AnimalType.BeefCowLactating, result.GroupType);
    }
}
