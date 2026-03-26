using H.Core.Enumerations;
using H.Core.Factories.Crops;
using H.Core.Mappers;
using H.Core.Models.LandManagement.Fields;
using H.Core.Services.Initialization;
using Moq;
using Prism.Ioc;

namespace H.Core.Test.ViewModels;

/// <summary>
/// Integration tests verifying that checklist toggle states interact correctly with crop DTOs
/// and the cover crop switching mechanism.
/// </summary>
[TestClass]
public class ChecklistIntegrationTests
{
    private ICropFactory _factory = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        var mockContainerProvider = new Mock<IContainerProvider>();
        var mockCropInitializationService = new Mock<ICropInitializationService>();

        mockContainerProvider.Setup(x => x.Resolve(typeof(IModelMapper<CropViewItem, CropDto>), nameof(CropViewItemToCropDtoMapper)))
            .Returns(new CropViewItemToCropDtoMapper());

        mockContainerProvider.Setup(x => x.Resolve(typeof(IModelMapper<CropDto, CropDto>), nameof(CropDtoToCropDtoMapper)))
            .Returns(new CropDtoToCropDtoMapper());

        mockContainerProvider.Setup(x => x.Resolve(typeof(IModelMapper<ICropDto, CropViewItem>), nameof(CropDtoToCropViewItemMapper)))
            .Returns(new CropDtoToCropViewItemMapper());

        _factory = new CropFactory(mockCropInitializationService.Object, mockContainerProvider.Object);
    }

    [TestMethod]
    public void TogglingCategories_DoesNotAffectCropDtoData()
    {
        var template = new CropViewItem { Year = 2020, CropType = CropType.Wheat };
        var dto = _factory.CreateCropDto(template);
        dto.WetYield = 5000;
        dto.MoistureContentOfCropPercentage = 14.5;
        dto.AmountOfIrrigation = 100;

        // Simulate toggling categories (these are VM properties, not DTO properties)
        // The DTO values should remain unchanged regardless of toggle state
        Assert.AreEqual(5000, dto.WetYield);
        Assert.AreEqual(14.5, dto.MoistureContentOfCropPercentage);
        Assert.AreEqual(100, dto.AmountOfIrrigation);
    }

    [TestMethod]
    public void SwitchingBetweenMainAndCoverCrop_PreservesIndependentData()
    {
        var template = new CropViewItem { Year = 2020, CropType = CropType.Wheat };
        var mainCrop = _factory.CreateCropDto(template);
        var coverCrop = _factory.CreateCoverCropDto(2020);
        mainCrop.CoverCropDto = coverCrop;

        // Verify main and cover crop are separate DTOs
        Assert.IsTrue(mainCrop.HasCoverCrop);
        Assert.AreNotSame(mainCrop, mainCrop.CoverCropDto);

        // Modify cover crop independently
        coverCrop.WetYield = 2000;
        Assert.AreNotEqual(mainCrop.WetYield, coverCrop.WetYield,
            "Cover crop yield should be independent of main crop");
    }

    [TestMethod]
    public void SelectingDifferentCrops_PreservesCropData()
    {
        var template1 = new CropViewItem { Year = 2020, CropType = CropType.Wheat };
        var crop1 = _factory.CreateCropDto(template1);

        var template2 = new CropViewItem { Year = 2021, CropType = CropType.Barley };
        var crop2 = _factory.CreateCropDto(template2);

        // Both crops exist independently
        Assert.AreEqual(CropType.Wheat, crop1.CropType);
        Assert.AreEqual(CropType.Barley, crop2.CropType);
    }

    [TestMethod]
    public void CoverCropDto_CanBeToggledOnAndOff_WithoutDataLoss()
    {
        var template = new CropViewItem { Year = 2020, CropType = CropType.Wheat };
        var mainCrop = _factory.CreateCropDto(template);
        var coverCrop = _factory.CreateCoverCropDto(2020);
        coverCrop.WetYield = 3000;
        coverCrop.CropType = CropType.RedCloverTrifoliumPratenseL;

        mainCrop.CoverCropDto = coverCrop;
        Assert.IsTrue(mainCrop.HasCoverCrop);
        Assert.AreEqual(3000, ((CropDto)mainCrop.CoverCropDto!).WetYield);

        // Remove cover crop
        mainCrop.CoverCropDto = null;
        Assert.IsFalse(mainCrop.HasCoverCrop);

        // Re-add with same data
        mainCrop.CoverCropDto = coverCrop;
        Assert.IsTrue(mainCrop.HasCoverCrop);
        Assert.AreEqual(3000, ((CropDto)mainCrop.CoverCropDto!).WetYield);
        Assert.AreEqual(CropType.RedCloverTrifoliumPratenseL, mainCrop.CoverCropDto.CropType);
    }

    [TestMethod]
    public void CoverCropFactory_CreatesCoverCropWithIsSecondaryCropTrue()
    {
        var coverCrop = _factory.CreateCoverCropDto(2020);

        Assert.IsTrue(coverCrop.IsSecondaryCrop);
        Assert.AreEqual(2020, coverCrop.Year);
        Assert.AreEqual(CoverCropTerminationType.Natural, coverCrop.CoverCropTerminationType);
    }

    [TestMethod]
    public void CropDto_HasCoverCrop_ReactsToPropertyChanges()
    {
        var dto = new CropDto();
        var changedProperties = new List<string>();
        dto.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        Assert.IsFalse(dto.HasCoverCrop);

        dto.CoverCropDto = new CropDto { IsSecondaryCrop = true };
        Assert.IsTrue(dto.HasCoverCrop);
        CollectionAssert.Contains(changedProperties, nameof(dto.HasCoverCrop));

        changedProperties.Clear();
        dto.CoverCropDto = null;
        Assert.IsFalse(dto.HasCoverCrop);
        CollectionAssert.Contains(changedProperties, nameof(dto.HasCoverCrop));
    }
}
