using AutoMapper;
using H.Core.Factories;
using H.Core.Factories.Animals;
using H.Core.Mappers;
using Moq;
using Prism.Ioc;

namespace H.Core.Test.Factories;

[TestClass]
public class AnimalComponentFactoryTests
{
    #region Fields

    private AnimalComponentFactory _sut = null!;

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

        mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), It.IsAny<string>())).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AnimalComponentDtoToAnimalComponentMapper>();
        }).CreateMapper());

        mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), It.IsAny<string>())).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AnimalComponentBaseToAnimalComponentDtoMapper>();
        }).CreateMapper());

        _sut = new AnimalComponentFactory(mockContainerProvider.Object);
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }

    #endregion

    [TestMethod]
    public void CreateAnimalComponentDto_ReturnsNonNullInstance()
    {
        // Arrange
        var inputDto = new AnimalComponentDto();

        // Act
        var dto = _sut.CreateDtoFromDtoTemplate(inputDto);

        // Assert
        Assert.IsNotNull(dto);
    }

    [TestMethod]
    public void CreateAnimalComponentDto_ReturnsNewInstanceEachTime()
    {
        // Arrange
        var inputDto = new AnimalComponentDto();

        // Act
        var dto1 = _sut.CreateDtoFromDtoTemplate(inputDto);
        var dto2 = _sut.CreateDtoFromDtoTemplate(inputDto);

        // Assert
        Assert.AreNotSame(dto1, dto2);
    }

    [TestMethod]
    public void CreateAnimalComponentDto_ImplementsIAnimalComponentDto()
    {
        // Arrange
        var inputDto = new AnimalComponentDto();

        // Act
        var dto = _sut.CreateDtoFromDtoTemplate(inputDto);

        // Assert
        Assert.IsInstanceOfType(dto, typeof(IAnimalComponentDto));
    }
}
