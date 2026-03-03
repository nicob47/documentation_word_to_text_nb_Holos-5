using AutoMapper;
using H.Core.Factories;
using H.Core.Factories.Animals;
using H.Core.Mappers;
using H.Core.Models;
using Moq;
using Prism.Ioc;

#nullable disable

namespace H.Core.Test.Factories;

[TestClass]
public class AnimalGroupFactoryTests
{
    #region Fields

    private AnimalGroupFactory _sut = null!;
    private Mock<IContainerProvider> _mockContainerProvider = null!;
    private IMapper _mockMapper = null!;

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
        _mockContainerProvider = new Mock<IContainerProvider>();

        _mockMapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AnimalGroupDtoToAnimalGroupDtoMapper>();
        }).CreateMapper();

        _mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), nameof(AnimalGroupDtoToAnimalGroupDtoMapper)))
            .Returns(_mockMapper);

        _sut = new AnimalGroupFactory(_mockContainerProvider.Object);
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }

    #endregion

    #region Constructor Tests

    [TestMethod]
    public void Constructor_WithValidContainerProvider_ShouldNotThrow()
    {
        // Arrange, Act & Assert
        var factory = new AnimalGroupFactory(_mockContainerProvider.Object);
        Assert.IsNotNull(factory);
    }

    [TestMethod]
    public void Constructor_ParameterlessConstructor_ShouldNotThrow()
    {
        // Arrange, Act & Assert
        var factory = new AnimalGroupFactory();
        Assert.IsNotNull(factory);
    }

    #endregion

    #region CreateDto Tests

    [TestMethod]
    public void CreateDto_ShouldReturnNonNullInstance()
    {
        // Act
        var result = _sut.CreateDto();

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void CreateDto_ShouldReturnAnimalGroupDto()
    {
        // Act
        var result = _sut.CreateDto();

        // Assert
        Assert.IsInstanceOfType(result, typeof(AnimalGroupDto));
    }

    [TestMethod]
    public void CreateDto_ShouldReturnNewInstanceEachTime()
    {
        // Act
        var dto1 = _sut.CreateDto();
        var dto2 = _sut.CreateDto();

        // Assert
        Assert.AreNotSame(dto1, dto2);
    }

    [TestMethod]
    public void CreateDto_ShouldImplementIAnimalGroupDto()
    {
        // Act
        var result = _sut.CreateDto();

        // Assert
        Assert.IsInstanceOfType(result, typeof(IAnimalGroupDto));
    }

    #endregion

    #region CreateDto with Farm Tests

    [TestMethod]
    public void CreateDtoWithFarm_ShouldReturnNonNullInstance()
    {
        // Arrange
        var farm = new Farm();

        // Act
        var result = _sut.CreateDto(farm);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void CreateDtoWithFarm_ShouldReturnAnimalGroupDto()
    {
        // Arrange
        var farm = new Farm();

        // Act
        var result = _sut.CreateDto(farm);

        // Assert
        Assert.IsInstanceOfType(result, typeof(AnimalGroupDto));
    }

    [TestMethod]
    public void CreateDtoWithFarm_WithNullFarm_ShouldReturnNonNullInstance()
    {
        // Act
        var result = _sut.CreateDto((Farm)null);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void CreateDtoWithFarm_ShouldReturnNewInstanceEachTime()
    {
        // Arrange
        var farm = new Farm();

        // Act
        var dto1 = _sut.CreateDto(farm);
        var dto2 = _sut.CreateDto(farm);

        // Assert
        Assert.AreNotSame(dto1, dto2);
    }

    #endregion

    #region CreateDtoFromDtoTemplate Tests

    [TestMethod]
    public void CreateDtoFromDtoTemplate_WithValidTemplate_ShouldReturnNonNullInstance()
    {
        // Arrange
        var template = new AnimalGroupDto();

        // Act
        var result = _sut.CreateDtoFromDtoTemplate(template);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void CreateDtoFromDtoTemplate_ShouldReturnAnimalGroupDto()
    {
        // Arrange
        var template = new AnimalGroupDto();

        // Act
        var result = _sut.CreateDtoFromDtoTemplate(template);

        // Assert
        Assert.IsInstanceOfType(result, typeof(AnimalGroupDto));
    }

    [TestMethod]
    public void CreateDtoFromDtoTemplate_ShouldReturnNewInstanceEachTime()
    {
        // Arrange
        var template = new AnimalGroupDto();

        // Act
        var dto1 = _sut.CreateDtoFromDtoTemplate(template);
        var dto2 = _sut.CreateDtoFromDtoTemplate(template);

        // Assert
        Assert.AreNotSame(dto1, dto2);
    }

    [TestMethod]
    public void CreateDtoFromDtoTemplate_ShouldImplementIAnimalGroupDto()
    {
        // Arrange
        var template = new AnimalGroupDto();

        // Act
        var result = _sut.CreateDtoFromDtoTemplate(template);

        // Assert
        Assert.IsInstanceOfType(result, typeof(IAnimalGroupDto));
    }

    [TestMethod]
    public void CreateDtoFromDtoTemplate_WithNullTemplate_ShouldReturnNonNullInstance()
    {
        // Act
        var result = _sut.CreateDtoFromDtoTemplate(null);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void CreateDtoFromDtoTemplate_WithParameterlessConstructor_ShouldReturnNonNullInstance()
    {
        // Arrange
        var factory = new AnimalGroupFactory(); // No mapper injected
        var template = new AnimalGroupDto();

        // Act
        var result = factory.CreateDtoFromDtoTemplate(template);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void CreateDtoFromDtoTemplate_WithParameterlessConstructor_ShouldNotUseMapper()
    {
        // Arrange
        var factory = new AnimalGroupFactory(); // No mapper injected
        var template = new AnimalGroupDto { Name = "Test Template" };

        // Act
        var result = factory.CreateDtoFromDtoTemplate(template);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(AnimalGroupDto));
        // Since no mapper is available, the result should be a new empty instance
        Assert.AreNotEqual(template.Name, ((AnimalGroupDto)result).Name);
    }

    #endregion
}
