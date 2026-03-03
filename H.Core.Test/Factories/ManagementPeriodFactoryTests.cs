using AutoMapper;
using H.Core.Factories;
using H.Core.Factories.Animals;
using H.Core.Mappers;
using H.Core.Models.Animals;
using Moq;
using Prism.Ioc;
using IManagementPeriodDto = H.Core.Factories.Animals.IManagementPeriodDto;

#nullable disable

namespace H.Core.Test.Factories;

[TestClass]
public class ManagementPeriodFactoryTests
{
    private Mock<IContainerProvider> _mockContainerProvider = null!;
    private ManagementPeriodFactory _factory = null!;

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

        // Setup mappers to return a working IMapper for each required profile
        _mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), nameof(ManagementPeriodDtoToManagementPeriodDtoMapper))).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ManagementPeriodDtoToManagementPeriodDtoMapper>();
        }).CreateMapper());

        _mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), nameof(ManagementPeriodToManagementPeriodDtoMapper))).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ManagementPeriodToManagementPeriodDtoMapper>();
        }).CreateMapper());

        _mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), nameof(ManagementPeriodDtoToManagementPeriodMapper))).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ManagementPeriodDtoToManagementPeriodMapper>();
        }).CreateMapper());

        _factory = new ManagementPeriodFactory(_mockContainerProvider.Object);
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }

    #endregion

    [TestMethod]
    public void Constructor_WithNullContainerProvider_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => new ManagementPeriodFactory(null));
    }

    [TestMethod]
    public void CreateManagementPeriodDto_ReturnsNewInstance()
    {
        // Act
        var dto = _factory.CreateManagementPeriodDto();

        // Assert
        Assert.IsNotNull(dto);
        Assert.IsInstanceOfType(dto, typeof(IManagementPeriodDto));
        Assert.AreEqual("New Management Period", dto.Name);
        Assert.IsTrue(dto.NumberOfDays > 0);
    }

    [TestMethod]
    public void CreateManagementPeriodDto_FromTemplate_ReturnsNewInstanceWithSameValues()
    {
        // Arrange
        var template = new ManagementPeriodDto();
        template.Name = "Test Period";
        template.Start = new DateTime(2024, 1, 1);
        template.End = new DateTime(2024, 12, 31);
        template.NumberOfDays = 366;

        // Act
        var dto = _factory.CreateDtoFromDtoTemplate(template);

        // Assert
        Assert.IsNotNull(dto);
        Assert.IsInstanceOfType(dto, typeof(IManagementPeriodDto));
        Assert.AreEqual("Test Period", dto.Name);
        Assert.AreEqual(template.Start, dto.Start);
        Assert.AreEqual(template.End, dto.End);
        Assert.AreEqual(template.NumberOfDays, dto.NumberOfDays);
        Assert.AreNotSame(template, dto); // Should be a different instance
    }

    [TestMethod]
    public void CreateManagementPeriodDto_FromDomainModel_ReturnsNewInstanceWithSameValues()
    {
        // Arrange
        var managementPeriod = new ManagementPeriod();
        managementPeriod.Name = "Test Domain Period";
        managementPeriod.Start = new DateTime(2024, 3, 1);
        managementPeriod.End = new DateTime(2024, 5, 31);
        managementPeriod.NumberOfDays = 92;

        // Act
        var dto = _factory.CreateManagementPeriodDto(managementPeriod);

        // Assert
        Assert.IsNotNull(dto);
        Assert.IsInstanceOfType(dto, typeof(IManagementPeriodDto));
        Assert.AreEqual("Test Domain Period", dto.Name);
        Assert.AreEqual(managementPeriod.Start, dto.Start);
        Assert.AreEqual(managementPeriod.End, dto.End);
        Assert.AreEqual(managementPeriod.NumberOfDays, dto.NumberOfDays);
    }

    [TestMethod]
    public void CreateManagementPeriod_FromDto_ReturnsNewDomainModelWithSameValues()
    {
        // Arrange
        var dto = new ManagementPeriodDto();
        dto.Name = "Test DTO to Domain Period";
        dto.Start = new DateTime(2024, 4, 1);
        dto.End = new DateTime(2024, 6, 30);
        dto.NumberOfDays = 91;

        // Act
        var managementPeriod = _factory.CreateManagementPeriod(dto);

        // Assert
        Assert.IsNotNull(managementPeriod);
        Assert.IsInstanceOfType(managementPeriod, typeof(ManagementPeriod));
        Assert.AreEqual("Test DTO to Domain Period", managementPeriod.Name);
        Assert.AreEqual(dto.Start, managementPeriod.Start);
        Assert.AreEqual(dto.End, managementPeriod.End);
        Assert.AreEqual(dto.NumberOfDays, managementPeriod.NumberOfDays);
    }
}
