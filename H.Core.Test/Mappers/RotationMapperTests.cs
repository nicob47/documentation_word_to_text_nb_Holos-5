using H.Core.Factories.Rotations;
using H.Core.Mappers;
using H.Core.Models.LandManagement.Fields;
using H.Core.Models.LandManagement.Rotation;

namespace H.Core.Test.Mappers;

[TestClass]
public class RotationMapperTests
{
    [TestMethod]
    public void RotationComponentToDto_MapsFieldArea()
    {
        var mapper = new RotationComponentToRotationComponentDtoMapper();
        var source = new RotationComponent
        {
            Name = "Rotation A",
            FieldSystemComponent = new FieldSystemComponent
            {
                FieldArea = 42.5
            }
        };

        var result = mapper.Map(source);

        Assert.AreEqual("Rotation A", result.Name);
        Assert.AreEqual(42.5, result.FieldArea);
    }

    [TestMethod]
    public void RotationDtoToComponent_MapsProperties()
    {
        var mapper = new RotationComponentDtoToRotationComponentMapper();
        var source = new RotationComponentDto
        {
            Name = "Rotation B",
            StartYear = 2020,
            EndYear = 2025
        };

        var result = mapper.Map(source);

        Assert.AreEqual("Rotation B", result.Name);
        Assert.AreEqual(2020, result.StartYear);
        Assert.AreEqual(2025, result.EndYear);
    }
}
