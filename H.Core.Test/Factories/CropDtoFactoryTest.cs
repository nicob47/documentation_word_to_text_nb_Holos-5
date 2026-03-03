using AutoMapper;
using H.Core.Factories.Crops;
using H.Core.Mappers;
using H.Core.Models;
using H.Core.Services.Initialization;
using Moq;
using Prism.Ioc;

namespace H.Core.Test.Factories;

[TestClass]
public class CropDtoFactoryTest
{
    #region Fields

    private ICropFactory _factory = null!;

    #endregion

    #region Initialization

    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
    }

    [TestInitialize]
    public void TestInitialize()
    {
        var mockContainerProvider = new Mock<IContainerProvider>();
        var mockCropInitializationService = new Mock<ICropInitializationService>();

        // Setup mappers to return a working IMapper for each required profile
        mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), nameof(CropViewItemToCropDtoMapper))).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CropViewItemToCropDtoMapper>();
        }).CreateMapper());

        mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), nameof(CropDtoToCropDtoMapper))).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CropDtoToCropDtoMapper>();
        }).CreateMapper());

        mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), nameof(CropDtoToCropViewItemMapper))).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CropDtoToCropViewItemMapper>();
        }).CreateMapper());

        _factory = new CropFactory(mockCropInitializationService.Object, mockContainerProvider.Object);
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }

    #endregion

    #region Tests

    [TestMethod]
    public void CreateReturnsNonNull()
    {
        var result = _factory.CreateDto(new Farm());

        Assert.IsNotNull(result);
    } 

    #endregion
}
