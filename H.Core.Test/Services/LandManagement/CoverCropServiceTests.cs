using H.Core.Enumerations;
using H.Core.Factories.Crops;
using H.Core.Factories.Fields;
using H.Core.Models.LandManagement.Fields;
using H.Core.Services.Animals;
using H.Core.Services.LandManagement.Fields;
using Microsoft.Extensions.Logging;
using Moq;

#nullable disable

namespace H.Core.Test.Services.LandManagement;

[TestClass]
public class CoverCropServiceTests
{
    private IFieldComponentService _fieldComponentService = null!;
    private Mock<ICropFactory> _mockCropFactory = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        var mockFieldFactory = new Mock<IFieldFactory>();
        _mockCropFactory = new Mock<ICropFactory>();
        var mockCropTransferService = new Mock<ITransferService<CropViewItem, CropDto>>();
        var mockFieldTransferService = new Mock<ITransferService<FieldSystemComponent, FieldSystemComponentDto>>();
        var mockLogger = new Mock<ILogger>();

        // Setup CreateCropViewItem to return a CropViewItem with matching Guid
        _mockCropFactory
            .Setup(x => x.CreateCropViewItem(It.IsAny<ICropDto>()))
            .Returns((ICropDto dto) => new CropViewItem
            {
                Guid = dto.Guid,
                CropType = dto.CropType,
                Year = dto.Year,
                IsSecondaryCrop = true
            });

        _fieldComponentService = new FieldComponentService(
            mockFieldFactory.Object,
            _mockCropFactory.Object,
            mockLogger.Object,
            mockCropTransferService.Object,
            mockFieldTransferService.Object
        );
    }

    [TestMethod]
    public void AddCoverCropToSystem_AddsToCoverCropsCollection()
    {
        var field = new FieldSystemComponent();
        var coverDto = new CropDto
        {
            IsSecondaryCrop = true,
            CropType = CropType.RedCloverTrifoliumPratenseL,
            Year = 2025,
        };

        _fieldComponentService.AddCoverCropToSystem(field, coverDto);

        Assert.AreEqual(1, field.CoverCrops.Count);
        Assert.IsTrue(field.CoverCrops[0].IsSecondaryCrop);
    }

    [TestMethod]
    public void RemoveCoverCropFromSystem_RemovesFromCollection()
    {
        var field = new FieldSystemComponent();
        var coverDto = new CropDto
        {
            IsSecondaryCrop = true,
            CropType = CropType.RedCloverTrifoliumPratenseL,
            Year = 2025,
        };

        _fieldComponentService.AddCoverCropToSystem(field, coverDto);
        Assert.AreEqual(1, field.CoverCrops.Count);

        _fieldComponentService.RemoveCoverCropFromSystem(field, coverDto);
        Assert.AreEqual(0, field.CoverCrops.Count);
    }

    [TestMethod]
    public void AddCoverCropToSystem_NullField_DoesNotThrow()
    {
        var coverDto = new CropDto { IsSecondaryCrop = true };

        _fieldComponentService.AddCoverCropToSystem(null, coverDto);
    }

    [TestMethod]
    public void AddCoverCropToSystem_NullDto_DoesNotThrow()
    {
        var field = new FieldSystemComponent();

        _fieldComponentService.AddCoverCropToSystem(field, null);

        Assert.AreEqual(0, field.CoverCrops.Count);
    }

    [TestMethod]
    public void RemoveCoverCropFromSystem_NonExistentDto_DoesNotThrow()
    {
        var field = new FieldSystemComponent();
        var coverDto = new CropDto { IsSecondaryCrop = true };

        _fieldComponentService.RemoveCoverCropFromSystem(field, coverDto);

        Assert.AreEqual(0, field.CoverCrops.Count);
    }
}
