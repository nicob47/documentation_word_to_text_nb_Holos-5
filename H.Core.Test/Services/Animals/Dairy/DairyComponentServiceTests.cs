using AutoMapper;
using H.Core.Enumerations;
using H.Core.Factories.Animals;
using H.Core.Factories.Animals.Dairy;
using H.Core.Mappers;
using H.Core.Models;
using H.Core.Models.Animals;
using H.Core.Models.Animals.Dairy;
using H.Core.Services.Animals.Dairy;
using Microsoft.Extensions.Logging;
using Moq;
using Prism.Ioc;

#nullable disable

namespace H.Core.Test.Services.Animals.Dairy;

/// <summary>
/// Unit tests for DairyComponentService
/// Tests DTO conversion, animal group generation, and data preservation logic
/// </summary>
[TestClass]
public class DairyComponentServiceTests
{
    #region Fields

    private DairyComponentService _sut = null!;
    private Mock<ILogger> _mockLogger = null!;
    private Mock<IContainerProvider> _mockContainerProvider = null!;
    private IMapper _dairyMapper = null!;
    private IMapper _animalGroupMapper = null!;
    private Farm _testFarm = null!;
    private DairyComponent _testDairyComponent = null!;
    private DairyComponentDto _testDairyComponentDto = null!;

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
        // Setup AutoMapper configurations with all required profiles
        var dairyMapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AnimalComponentBaseToAnimalComponentDtoMapper>();
            cfg.AddProfile<AnimalComponentDtoToAnimalComponentMapper>();
            cfg.AddProfile<DairyComponentToDtoMapper>();
        });
        _dairyMapper = dairyMapperConfig.CreateMapper();

        var animalGroupMapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AnimalGroupToAnimalGroupDtoMapper>();
        });
        _animalGroupMapper = animalGroupMapperConfig.CreateMapper();

        // Setup mocks
        _mockLogger = new Mock<ILogger>();
        _mockContainerProvider = new Mock<IContainerProvider>();

        // Configure container provider to return mappers
        _mockContainerProvider
            .Setup(x => x.Resolve(typeof(IMapper), nameof(DairyComponentToDtoMapper)))
            .Returns(_dairyMapper);

        _mockContainerProvider
            .Setup(x => x.Resolve(typeof(IMapper), nameof(AnimalGroupToAnimalGroupDtoMapper)))
            .Returns(_animalGroupMapper);

        // Create test data
        _testFarm = new Farm { Name = "Test Dairy Farm" };
        _testDairyComponent = new DairyComponent
        {
            Name = "Test Dairy Herd",
            Guid = Guid.NewGuid()
        };

        _testDairyComponentDto = new DairyComponentDto
        {
            Name = "Test Dairy Herd",
            TotalMilkingCows = 100,
            ReplacementRate = 30.0,
            CalvingIntervalMonths = 14,
            DryPeriodDays = 60,
            CalfMortalityRate = 5.0,
            FemaleCalfRatio = 50.0,
            DefaultMilkProduction = 28.0,
            DefaultMilkFatContent = 3.7,
            DefaultMilkProteinContent = 3.1
        };

        // Create system under test
        _sut = new DairyComponentService(_mockLogger.Object, _mockContainerProvider.Object);
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }

    #endregion

    #region Constructor Tests

    [TestMethod]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Arrange, Act & Assert (done in TestInitialize)
        Assert.IsNotNull(_sut);
    }

    [TestMethod]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            // Arrange & Act
            new DairyComponentService(null, _mockContainerProvider.Object);
        });
    }

    [TestMethod]
    public void Constructor_WithNullContainerProvider_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            // Arrange & Act
            new DairyComponentService(_mockLogger.Object, null);
        });
    }

    #endregion

    #region InitializeComponent Tests

    [TestMethod]
    public void InitializeComponent_WithValidComponent_SetsProperties()
    {
        // Arrange
        _testDairyComponent.IsInitialized = false;

        // Act
        _sut.InitializeComponent(_testFarm, _testDairyComponent);

        // Assert
        Assert.IsTrue(_testDairyComponent.IsInitialized);
        Assert.IsNotNull(_testDairyComponent.Name);
    }

    [TestMethod]
    public void InitializeComponent_WithNullComponent_DoesNotThrow()
    {
        // Arrange
        DairyComponent nullComponent = null;

        // Act & Assert - Should not throw
        _sut.InitializeComponent(_testFarm, nullComponent);
    }

    #endregion

    #region TransferToDairyComponentDto Tests

    [TestMethod]
    public void TransferToDairyComponentDto_WithValidComponent_ReturnsDto()
    {
        // Arrange
        _testDairyComponent.Name = "Dairy Herd 1";

        // Act
        var result = _sut.TransferToDairyComponentDto(_testDairyComponent);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(DairyComponentDto));
        Assert.AreEqual(_testDairyComponent.Name, result.Name);
    }

    [TestMethod]
    public void TransferToDairyComponentDto_WithAnimalGroups_ConvertsToAnimalGroupDtos()
    {
        // Arrange
        _testDairyComponent.Groups.Add(new AnimalGroup
        {
            Name = "Lactating Cows",
            GroupType = AnimalType.DairyLactatingCow,
            Guid = Guid.NewGuid()
        });
        _testDairyComponent.Groups.Add(new AnimalGroup
        {
            Name = "Dry Cows",
            GroupType = AnimalType.DairyDryCow,
            Guid = Guid.NewGuid()
        });

        // Act
        var result = _sut.TransferToDairyComponentDto(_testDairyComponent);

        // Assert
        Assert.AreEqual(2, result.AnimalGroupDtos.Count);
        Assert.AreEqual("Lactating Cows", result.AnimalGroupDtos[0].Name);
        Assert.AreEqual(AnimalType.DairyLactatingCow, result.AnimalGroupDtos[0].GroupType);
        Assert.AreEqual("Dry Cows", result.AnimalGroupDtos[1].Name);
        Assert.AreEqual(AnimalType.DairyDryCow, result.AnimalGroupDtos[1].GroupType);
    }

    [TestMethod]
    public void TransferToDairyComponentDto_WithEmptyGroups_ReturnsEmptyDtoCollection()
    {
        // Arrange
        _testDairyComponent.Groups.Clear();

        // Act
        var result = _sut.TransferToDairyComponentDto(_testDairyComponent);

        // Assert
        Assert.IsNotNull(result.AnimalGroupDtos);
        Assert.AreEqual(0, result.AnimalGroupDtos.Count);
    }

    #endregion

    #region TransferDairyDtoToSystem Tests

    [TestMethod]
    public void TransferDairyDtoToSystem_WithValidDto_UpdatesComponent()
    {
        // Arrange
        _testDairyComponentDto.Name = "Updated Dairy Herd";

        // Act
        var result = _sut.TransferDairyDtoToSystem(_testDairyComponentDto, _testDairyComponent);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Updated Dairy Herd", result.Name);
    }

    [TestMethod]
    public void TransferDairyDtoToSystem_WithAnimalGroupDtos_ConvertsToAnimalGroups()
    {
        // Arrange
        _testDairyComponentDto.AnimalGroupDtos.Clear();
        _testDairyComponentDto.AnimalGroupDtos.Add(new AnimalGroupDto
        {
            Name = "Test Group 1",
            GroupType = AnimalType.DairyHeifers,
            Guid = Guid.NewGuid()
        });
        _testDairyComponentDto.AnimalGroupDtos.Add(new AnimalGroupDto
        {
            Name = "Test Group 2",
            GroupType = AnimalType.DairyCalves,
            Guid = Guid.NewGuid()
        });

        // Act
        var result = _sut.TransferDairyDtoToSystem(_testDairyComponentDto, _testDairyComponent);

        // Assert
        Assert.AreEqual(2, result.Groups.Count);
        Assert.AreEqual("Test Group 1", result.Groups[0].Name);
        Assert.AreEqual(AnimalType.DairyHeifers, result.Groups[0].GroupType);
        Assert.AreEqual("Test Group 2", result.Groups[1].Name);
        Assert.AreEqual(AnimalType.DairyCalves, result.Groups[1].GroupType);
    }

    [TestMethod]
    public void TransferDairyDtoToSystem_ClearsExistingGroups_BeforeAddingNew()
    {
        // Arrange
        _testDairyComponent.Groups.Add(new AnimalGroup { Name = "Old Group" });
        _testDairyComponentDto.AnimalGroupDtos.Add(new AnimalGroupDto
        {
            Name = "New Group",
            GroupType = AnimalType.DairyLactatingCow
        });

        // Act
        var result = _sut.TransferDairyDtoToSystem(_testDairyComponentDto, _testDairyComponent);

        // Assert
        Assert.AreEqual(1, result.Groups.Count);
        Assert.AreEqual("New Group", result.Groups[0].Name);
    }

    #endregion

    #region GenerateAnimalGroups Tests

    [TestMethod]
    public void GenerateAnimalGroups_WithEmptyComponent_CreatesFourGroups()
    {
        // Arrange
        _testDairyComponent.Groups.Clear();

        // Act
        _sut.GenerateAnimalGroups(_testDairyComponentDto, _testDairyComponent);

        // Assert
        Assert.AreEqual(4, _testDairyComponent.Groups.Count);
        Assert.AreEqual(4, _testDairyComponentDto.AnimalGroupDtos.Count);
    }

    [TestMethod]
    public void GenerateAnimalGroups_CreatesCalfGroup_WithCorrectProperties()
    {
        // Arrange
        _testDairyComponent.Groups.Clear();
        _testDairyComponentDto.TotalMilkingCows = 100;

        // Act
        _sut.GenerateAnimalGroups(_testDairyComponentDto, _testDairyComponent);

        // Assert
        var calfGroup = _testDairyComponent.Groups.FirstOrDefault(g => g.GroupType == AnimalType.DairyCalves);
        Assert.IsNotNull(calfGroup);
        Assert.AreEqual("Dairy Calves", calfGroup.Name);
        Assert.IsTrue(calfGroup.ManagementPeriods.Count > 0);

        var period = calfGroup.ManagementPeriods[0];
        Assert.AreEqual(45, period.StartWeight);
        Assert.AreEqual(120, period.EndWeight);
        Assert.AreEqual(0.6, period.PeriodDailyGain);
        Assert.AreEqual(HousingType.HousedInBarn, period.HousingDetails.HousingType);
        Assert.AreEqual(ManureStateType.SolidStorage, period.ManureDetails.StateType);
    }

    [TestMethod]
    public void GenerateAnimalGroups_CreatesHeiferGroup_WithCorrectProperties()
    {
        // Arrange
        _testDairyComponent.Groups.Clear();

        // Act
        _sut.GenerateAnimalGroups(_testDairyComponentDto, _testDairyComponent);

        // Assert
        var heiferGroup = _testDairyComponent.Groups.FirstOrDefault(g => g.GroupType == AnimalType.DairyHeifers);
        Assert.IsNotNull(heiferGroup);
        Assert.AreEqual("Dairy Heifers", heiferGroup.Name);

        var period = heiferGroup.ManagementPeriods[0];
        Assert.AreEqual(120, period.StartWeight);
        Assert.AreEqual(600, period.EndWeight);
        Assert.AreEqual(0.8, period.PeriodDailyGain);
    }

    [TestMethod]
    public void GenerateAnimalGroups_CreatesLactatingGroup_WithProductionDefaults()
    {
        // Arrange
        _testDairyComponent.Groups.Clear();
        _testDairyComponentDto.DefaultMilkProduction = 30.0;
        _testDairyComponentDto.DefaultMilkFatContent = 4.0;
        _testDairyComponentDto.DefaultMilkProteinContent = 3.5;

        // Act
        _sut.GenerateAnimalGroups(_testDairyComponentDto, _testDairyComponent);

        // Assert
        var lactatingGroup = _testDairyComponent.Groups.FirstOrDefault(g => g.GroupType == AnimalType.DairyLactatingCow);
        Assert.IsNotNull(lactatingGroup);
        Assert.AreEqual("Lactating Cows", lactatingGroup.Name);

        var period = lactatingGroup.ManagementPeriods[0];
        Assert.AreEqual(30.0, period.MilkProduction);
        Assert.AreEqual(4.0, period.MilkFatContent);
        Assert.AreEqual(3.5, period.MilkProteinContentAsPercentage);
        Assert.AreEqual(ManureStateType.LiquidWithNaturalCrust, period.ManureDetails.StateType);
    }

    [TestMethod]
    public void GenerateAnimalGroups_CreatesDryGroup_WithDryPeriodFromDto()
    {
        // Arrange
        _testDairyComponent.Groups.Clear();
        _testDairyComponentDto.DryPeriodDays = 90;

        // Act
        _sut.GenerateAnimalGroups(_testDairyComponentDto, _testDairyComponent);

        // Assert
        var dryGroup = _testDairyComponent.Groups.FirstOrDefault(g => g.GroupType == AnimalType.DairyDryCow);
        Assert.IsNotNull(dryGroup);
        Assert.AreEqual("Dry Cows", dryGroup.Name);

        var period = dryGroup.ManagementPeriods[0];
        Assert.AreEqual(90, period.NumberOfDays);
        Assert.AreEqual(0, period.MilkProduction);
        Assert.AreEqual(650, period.StartWeight);
        Assert.AreEqual(700, period.EndWeight);
    }

    [TestMethod]
    public void GenerateAnimalGroups_UsesCalculatedAnimalCounts()
    {
        // Arrange
        _testDairyComponent.Groups.Clear();
        _testDairyComponentDto.TotalMilkingCows = 200;
        // Calculated values will be computed by the DTO

        // Act
        _sut.GenerateAnimalGroups(_testDairyComponentDto, _testDairyComponent);

        // Assert
        var calfGroup = _testDairyComponent.Groups.First(g => g.GroupType == AnimalType.DairyCalves);
        var heiferGroup = _testDairyComponent.Groups.First(g => g.GroupType == AnimalType.DairyHeifers);
        var lactatingGroup = _testDairyComponent.Groups.First(g => g.GroupType == AnimalType.DairyLactatingCow);
        var dryGroup = _testDairyComponent.Groups.First(g => g.GroupType == AnimalType.DairyDryCow);

        Assert.AreEqual(_testDairyComponentDto.CalculatedCalves, calfGroup.ManagementPeriods[0].NumberOfAnimals);
        Assert.AreEqual(_testDairyComponentDto.CalculatedHeifers, heiferGroup.ManagementPeriods[0].NumberOfAnimals);
        Assert.AreEqual(_testDairyComponentDto.CalculatedLactating, lactatingGroup.ManagementPeriods[0].NumberOfAnimals);
        Assert.AreEqual(_testDairyComponentDto.CalculatedDry, dryGroup.ManagementPeriods[0].NumberOfAnimals);
    }

    [TestMethod]
    public void GenerateAnimalGroups_WithExistingGroups_PreservesGroups_WhenForceRegenerationIsFalse()
    {
        // Arrange
        var existingGroup = new AnimalGroup
        {
            Name = "Existing Group",
            GroupType = AnimalType.DairyLactatingCow,
            Guid = Guid.NewGuid()
        };
        _testDairyComponent.Groups.Add(existingGroup);

        // Act
        _sut.GenerateAnimalGroups(_testDairyComponentDto, _testDairyComponent, forceRegeneration: false);

        // Assert
        Assert.AreEqual(1, _testDairyComponent.Groups.Count);
        Assert.AreEqual("Existing Group", _testDairyComponent.Groups[0].Name);
    }

    [TestMethod]
    public void GenerateAnimalGroups_WithExistingGroups_ReplacesGroups_WhenForceRegenerationIsTrue()
    {
        // Arrange
        _testDairyComponent.Groups.Add(new AnimalGroup { Name = "Old Group 1" });
        _testDairyComponent.Groups.Add(new AnimalGroup { Name = "Old Group 2" });
        _testDairyComponentDto.AnimalGroupDtos.Add(new AnimalGroupDto { Name = "Old DTO 1" });
        _testDairyComponentDto.AnimalGroupDtos.Add(new AnimalGroupDto { Name = "Old DTO 2" });

        // Act
        _sut.GenerateAnimalGroups(_testDairyComponentDto, _testDairyComponent, forceRegeneration: true);

        // Assert
        Assert.AreEqual(4, _testDairyComponent.Groups.Count);
        Assert.AreEqual(4, _testDairyComponentDto.AnimalGroupDtos.Count);
        Assert.IsFalse(_testDairyComponent.Groups.Any(g => g.Name == "Old Group 1"));
        Assert.IsFalse(_testDairyComponentDto.AnimalGroupDtos.Any(d => d.Name == "Old DTO 1"));
    }

    [TestMethod]
    public void GenerateAnimalGroups_WithNullDto_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            // Arrange & Act
            _sut.GenerateAnimalGroups(null, _testDairyComponent);
        });
    }

    [TestMethod]
    public void GenerateAnimalGroups_WithNullComponent_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            // Arrange & Act
            _sut.GenerateAnimalGroups(_testDairyComponentDto, null);
        });
    }

    [TestMethod]
    public void GenerateAnimalGroups_LogsInformationMessage()
    {
        // Arrange
        _testDairyComponent.Groups.Clear();
        _testDairyComponent.Name = "Test Dairy";

        // Act
        _sut.GenerateAnimalGroups(_testDairyComponentDto, _testDairyComponent);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Auto-generated 4 animal groups")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void GenerateAnimalGroups_LogsSkipMessage_WhenGroupsExistAndNoForceRegeneration()
    {
        // Arrange
        _testDairyComponent.Groups.Add(new AnimalGroup { Name = "Existing" });
        _testDairyComponent.Name = "Test Dairy";

        // Act
        _sut.GenerateAnimalGroups(_testDairyComponentDto, _testDairyComponent, forceRegeneration: false);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Skipping auto-generation")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void GenerateAnimalGroups_LogsWarning_WhenForcingRegeneration()
    {
        // Arrange
        _testDairyComponent.Groups.Add(new AnimalGroup { Name = "Existing" });
        _testDairyComponent.Name = "Test Dairy";

        // Act
        _sut.GenerateAnimalGroups(_testDairyComponentDto, _testDairyComponent, forceRegeneration: true);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Force regeneration requested")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void FullWorkflow_CreateDtoGenerateGroupsTransferBack_WorksCorrectly()
    {
        // Arrange
        _testDairyComponent.Name = "Integration Test Herd";
        _testDairyComponent.Groups.Clear();

        // Act - Step 1: Transfer to DTO
        var dto = _sut.TransferToDairyComponentDto(_testDairyComponent) as DairyComponentDto;
        Assert.IsNotNull(dto);
        Assert.AreEqual(0, dto.AnimalGroupDtos.Count);

        // Act - Step 2: Generate groups
        _sut.GenerateAnimalGroups(dto, _testDairyComponent);
        Assert.AreEqual(4, _testDairyComponent.Groups.Count);
        Assert.AreEqual(4, dto.AnimalGroupDtos.Count);

        // Act - Step 3: Transfer back to domain
        var updatedComponent = _sut.TransferDairyDtoToSystem(dto, _testDairyComponent);

        // Assert
        Assert.AreEqual(4, updatedComponent.Groups.Count);
        Assert.IsTrue(updatedComponent.Groups.Any(g => g.GroupType == AnimalType.DairyCalves));
        Assert.IsTrue(updatedComponent.Groups.Any(g => g.GroupType == AnimalType.DairyHeifers));
        Assert.IsTrue(updatedComponent.Groups.Any(g => g.GroupType == AnimalType.DairyLactatingCow));
        Assert.IsTrue(updatedComponent.Groups.Any(g => g.GroupType == AnimalType.DairyDryCow));
    }

    [TestMethod]
    public void DataPreservation_LoadingSavedComponent_PreservesGroups()
    {
        // Arrange - Simulate a saved component with configured groups
        var savedComponent = new DairyComponent
        {
            Name = "Saved Dairy Herd",
            Guid = Guid.NewGuid()
        };
        savedComponent.Groups.Add(new AnimalGroup
        {
            Name = "Custom Lactating Group",
            GroupType = AnimalType.DairyLactatingCow,
            Guid = Guid.NewGuid()
        });
        savedComponent.Groups.Add(new AnimalGroup
        {
            Name = "Custom Dry Group",
            GroupType = AnimalType.DairyDryCow,
            Guid = Guid.NewGuid()
        });

        // Act - Step 1: Load component (transfer to DTO)
        var dto = _sut.TransferToDairyComponentDto(savedComponent) as DairyComponentDto;

        // Act - Step 2: Try to auto-generate (should skip because groups exist)
        _sut.GenerateAnimalGroups(dto, savedComponent, forceRegeneration: false);

        // Assert
        Assert.AreEqual(2, savedComponent.Groups.Count);
        Assert.AreEqual("Custom Lactating Group", savedComponent.Groups[0].Name);
        Assert.AreEqual("Custom Dry Group", savedComponent.Groups[1].Name);
    }

    #endregion
}
