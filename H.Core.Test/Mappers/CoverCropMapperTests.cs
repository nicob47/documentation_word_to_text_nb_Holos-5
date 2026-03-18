using H.Core.Enumerations;
using H.Core.Factories.Crops;
using H.Core.Mappers;

namespace H.Core.Test.Mappers;

[TestClass]
public class CoverCropMapperTests
{
    [TestMethod]
    public void PropertyMapper_CopiesCoverCropProperties()
    {
        var source = new CropDto
        {
            IsSecondaryCrop = true,
            CoverCropTerminationType = CoverCropTerminationType.Mechanical,
            WetYield = 500,
            AmountOfIrrigation = 100,
        };

        var target = new CropDto();

        PropertyMapper.CopyTo(source, target);

        Assert.IsTrue(target.IsSecondaryCrop);
        Assert.AreEqual(CoverCropTerminationType.Mechanical, target.CoverCropTerminationType);
        Assert.AreEqual(500, target.WetYield);
        Assert.AreEqual(100, target.AmountOfIrrigation);
    }

    [TestMethod]
    public void CropDto_HasCoverCrop_ReturnsTrueWhenCoverCropDtoSet()
    {
        var parentDto = new CropDto();
        var coverDto = new CropDto { IsSecondaryCrop = true };

        parentDto.CoverCropDto = coverDto;

        Assert.IsTrue(parentDto.HasCoverCrop);
    }

    [TestMethod]
    public void CropDto_GroupedCropItems_UsesOnlyCoverCropTypes_WhenIsSecondaryCrop()
    {
        var dto = new CropDto { IsSecondaryCrop = true };

        var items = dto.GroupedCropItems;

        // GroupedCropItems should contain CropType values from the cover crop list
        var cropTypes = items.OfType<CropType>().ToList();
        var validCoverCropTypes = CropTypeExtensions.GetValidCoverCropTypes().ToList();

        Assert.IsTrue(cropTypes.Count > 0, "Should have at least one cover crop type");
        foreach (var cropType in cropTypes)
        {
            Assert.IsTrue(validCoverCropTypes.Contains(cropType),
                $"CropType {cropType} should be a valid cover crop type");
        }
    }

    [TestMethod]
    public void CropDto_GroupedCropItems_UsesDefaultTypes_WhenNotSecondaryCrop()
    {
        var dto = new CropDto();

        var items = dto.GroupedCropItems;

        var cropTypes = items.OfType<CropType>().ToList();

        // Default list includes Wheat, which is not a cover crop
        Assert.IsTrue(cropTypes.Contains(CropType.Wheat), "Default list should contain Wheat");
    }

    [TestMethod]
    public void CropDto_IsSecondaryCrop_InvalidatesGroupedCropItems()
    {
        var dto = new CropDto();

        // Access default grouped items
        var defaultItems = dto.GroupedCropItems;
        var defaultCropTypes = defaultItems.OfType<CropType>().ToList();
        Assert.IsTrue(defaultCropTypes.Contains(CropType.Wheat));

        // Switch to secondary crop
        dto.IsSecondaryCrop = true;
        var coverItems = dto.GroupedCropItems;
        var coverCropTypes = coverItems.OfType<CropType>().ToList();

        // Should no longer contain Wheat
        Assert.IsFalse(coverCropTypes.Contains(CropType.Wheat),
            "Cover crop list should not contain Wheat");
    }
}
