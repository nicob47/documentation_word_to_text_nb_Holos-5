using H.Core.Enumerations;
using H.Core.Factories.Crops;
using H.Core.Mappers;
using H.Core.Models.LandManagement.Fields;
using H.Core.Services.Initialization;
using Moq;
using Prism.Ioc;

namespace H.Core.Test.Factories;

[TestClass]
public class CoverCropDtoTests
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
    public void CreateCoverCropDto_SetsIsSecondaryCropTrue()
    {
        var dto = _factory.CreateCoverCropDto(2025);

        Assert.IsTrue(dto.IsSecondaryCrop);
    }

    [TestMethod]
    public void CreateCoverCropDto_DefaultsToNaturalTermination()
    {
        var dto = _factory.CreateCoverCropDto(2025);

        Assert.AreEqual(CoverCropTerminationType.Natural, dto.CoverCropTerminationType);
    }

    [TestMethod]
    public void CreateCoverCropDto_SetsYear()
    {
        var dto = _factory.CreateCoverCropDto(2023);

        Assert.AreEqual(2023, dto.Year);
    }

    [TestMethod]
    public void CreateCoverCropDto_UsesValidCoverCropTypes()
    {
        var dto = _factory.CreateCoverCropDto(2025);

        // Access GroupedCropItems to trigger cover crop type population
        var items = dto.GroupedCropItems;

        var validCoverCropTypes = CropTypeExtensions.GetValidCoverCropTypes().ToList();
        Assert.IsTrue(dto.ValidCropTypes.Count > 0);

        // All ValidCropTypes should be from the cover crop list
        foreach (var cropType in dto.ValidCropTypes)
        {
            Assert.IsTrue(validCoverCropTypes.Contains(cropType),
                $"CropType {cropType} should be a valid cover crop type");
        }
    }

    [TestMethod]
    public void CropDto_HasCoverCrop_ReturnsFalseByDefault()
    {
        var dto = new CropDto();

        Assert.IsFalse(dto.HasCoverCrop);
    }

    [TestMethod]
    public void CropDto_HasCoverCrop_ReturnsTrueWhenCoverCropSet()
    {
        var dto = new CropDto();
        dto.CoverCropDto = _factory.CreateCoverCropDto(2025);

        Assert.IsTrue(dto.HasCoverCrop);
    }

    [TestMethod]
    public void CropDto_HasCoverCrop_ReturnsFalseWhenCoverCropCleared()
    {
        var dto = new CropDto();
        dto.CoverCropDto = _factory.CreateCoverCropDto(2025);
        dto.CoverCropDto = null;

        Assert.IsFalse(dto.HasCoverCrop);
    }

    [TestMethod]
    public void CropDto_CoverCropProperties_CanBeSetAndRetrieved()
    {
        var dto = new CropDto
        {
            IsSecondaryCrop = true,
            CoverCropTerminationType = CoverCropTerminationType.Chemical,
        };

        Assert.IsTrue(dto.IsSecondaryCrop);
        Assert.AreEqual(CoverCropTerminationType.Chemical, dto.CoverCropTerminationType);
    }
}
