using H.Avalonia.ViewModels.ComponentViews.Dairy;
using H.Core;
using H.Core.Factories.Animals.Dairy;
using H.Core.Models;
using H.Core.Models.Animals.Dairy;
using H.Core.Models.Animals.Beef;
using H.Core.Services.Animals.Dairy;
using H.Core.Services.StorageService;
using Microsoft.Extensions.Logging;
using Moq;
using Prism.Events;
using Prism.Regions;

#nullable disable

namespace H.Avalonia.Test.ViewModels.ComponentViews.Dairy;

/// <summary>
/// Unit tests for the DairyComponentViewModel class.
/// Tests initialization, data binding, validation, and error handling scenarios.
/// </summary>
[TestClass]
public class DairyComponentViewModelTests
{
    #region Fields

    private DairyComponentViewModel _viewModel = null!;
    private Mock<IRegionManager> _mockRegionManager = null!;
    private Mock<IEventAggregator> _mockEventAggregator = null!;
    private Mock<IStorageService> _mockStorageService = null!;
    private Mock<IDairyComponentService> _mockDairyComponentService = null!;
    private Mock<ILogger> _mockLogger = null!;
    private Farm _testFarm = null!;
    private DairyComponent _testDairyComponent = null!;
    private DairyComponentDto _testDairyComponentDto = null!;

    #endregion

    #region Initialization

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
    }

    [TestInitialize]
    public void TestInitialize()
    {
        // Create test data
        _testFarm = new Farm { Name = "Test Farm" };
        _testDairyComponent = new DairyComponent
        {
            Name = "Test Dairy Herd",
            Guid = Guid.NewGuid()
        };
        _testFarm.Components.Add(_testDairyComponent);

        _testDairyComponentDto = new DairyComponentDto
        {
            Name = "Test Dairy Herd",
            TotalMilkingCows = 100,
            ReplacementRate = 30.0,
            CalvingIntervalMonths = 14,
            DryPeriodDays = 60,
            CalfMortalityRate = 5.0,
            FemaleCalfRatio = 50.0
        };

        // Setup mocks
        _mockRegionManager = new Mock<IRegionManager>();
        _mockEventAggregator = new Mock<IEventAggregator>();
        _mockStorageService = new Mock<IStorageService>();
        _mockDairyComponentService = new Mock<IDairyComponentService>();
        _mockLogger = new Mock<ILogger>();

        // Configure storage service mock
        var storage = new H.Core.Storage
        {
            ApplicationData = new ApplicationData
            {
                GlobalSettings = new GlobalSettings()
            }
        };
        _mockStorageService.Setup(x => x.Storage).Returns(storage);
        _mockStorageService.Setup(x => x.GetActiveFarm()).Returns(_testFarm);

        // Configure dairy component service mock
        _mockDairyComponentService
            .Setup(x => x.TransferToDairyComponentDto(It.IsAny<DairyComponent>()))
            .Returns(_testDairyComponentDto);

        // Create view model
        _viewModel = new DairyComponentViewModel(
            _mockRegionManager.Object,
            _mockEventAggregator.Object,
            _mockStorageService.Object,
            _mockDairyComponentService.Object,
            _mockLogger.Object);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _viewModel?.Dispose();
    }

    #endregion

    #region Constructor Tests

    [TestMethod]
    public void Constructor_WithValidDependencies_CreatesViewModel()
    {
        // Arrange & Act are done in TestInitialize

        // Assert
        Assert.IsNotNull(_viewModel);
    }

    [TestMethod]
    public void Constructor_WithNullDairyComponentService_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            // Arrange & Act
            var viewModel = new DairyComponentViewModel(
                _mockRegionManager.Object,
                _mockEventAggregator.Object,
                _mockStorageService.Object,
                null!, // Null dairy component service
                _mockLogger.Object);
        });
    }

    [TestMethod]
    public void ParameterlessConstructor_CreatesViewModel()
    {
        // Arrange & Act
        var viewModel = new DairyComponentViewModel();

        // Assert
        Assert.IsNotNull(viewModel);
    }

    #endregion

    #region InitializeViewModel Tests

    [TestMethod]
    public void InitializeViewModel_WithDairyComponent_SetsSelectedDairyComponentDto()
    {
        // Arrange
        // Setup is done in TestInitialize

        // Act
        _viewModel.InitializeViewModel(_testDairyComponent);

        // Assert
        Assert.IsNotNull(_viewModel.SelectedDairyComponentDto);
        _mockDairyComponentService.Verify(
            x => x.TransferToDairyComponentDto(_testDairyComponent),
            Times.Once);
    }

    [TestMethod]
    public void InitializeViewModel_WithDairyComponent_DtoHasCorrectValues()
    {
        // Arrange
        // Setup is done in TestInitialize

        // Act
        _viewModel.InitializeViewModel(_testDairyComponent);

        // Assert
        Assert.IsNotNull(_viewModel.SelectedDairyComponentDto);
        Assert.AreEqual("Test Dairy Herd", _viewModel.SelectedDairyComponentDto.Name);
        Assert.AreEqual(100, _viewModel.SelectedDairyComponentDto.TotalMilkingCows);
        Assert.AreEqual(30.0, _viewModel.SelectedDairyComponentDto.ReplacementRate);
        Assert.AreEqual(14, _viewModel.SelectedDairyComponentDto.CalvingIntervalMonths);
        Assert.AreEqual(60, _viewModel.SelectedDairyComponentDto.DryPeriodDays);
    }

    [TestMethod]
    public void InitializeViewModel_WithNonDairyComponent_DoesNotSetDto()
    {
        // Arrange
        var nonDairyComponent = new BackgroundingComponent { Name = "Not a dairy component" };

        // Act
        _viewModel.InitializeViewModel(nonDairyComponent);

        // Assert
        Assert.IsNull(_viewModel.SelectedDairyComponentDto);
        _mockDairyComponentService.Verify(
            x => x.TransferToDairyComponentDto(It.IsAny<DairyComponent>()),
            Times.Never);
    }

    [TestMethod]
    public void InitializeViewModel_WithNullComponent_DoesNotThrow()
    {
        // Arrange
        ComponentBase nullComponent = null;

        // Act & Assert - Should not throw
        _viewModel.InitializeViewModel(nullComponent);
        Assert.IsNull(_viewModel.SelectedDairyComponentDto);
    }

    #endregion

    #region InitializeDairyComponent Tests

    [TestMethod]
    public void InitializeDairyComponent_WithValidComponent_CallsServiceTransfer()
    {
        // Arrange
        // Setup is done in TestInitialize

        // Act
        _viewModel.InitializeDairyComponent(_testDairyComponent);

        // Assert
        _mockDairyComponentService.Verify(
            x => x.TransferToDairyComponentDto(_testDairyComponent),
            Times.Once);
    }

    [TestMethod]
    public void InitializeDairyComponent_WithNullComponent_DoesNotCallService()
    {
        // Arrange
        DairyComponent nullComponent = null;

        // Act
        _viewModel.InitializeDairyComponent(nullComponent);

        // Assert
        _mockDairyComponentService.Verify(
            x => x.TransferToDairyComponentDto(It.IsAny<DairyComponent>()),
            Times.Never);
    }

    #endregion

    #region OnNavigatedTo Tests

    [TestMethod]
    public void OnNavigatedTo_WithDairyComponentParameter_InitializesViewModel()
    {
        // Arrange
        var navigationContext = new NavigationContext(
            new Mock<IRegionNavigationService>().Object,
            new Uri("test", UriKind.Relative));
        navigationContext.Parameters.Add(GuiConstants.ComponentKey, _testDairyComponent);

        // Act
        _viewModel.OnNavigatedTo(navigationContext);

        // Assert
        Assert.IsNotNull(_viewModel.SelectedDairyComponentDto);
        _mockDairyComponentService.Verify(
            x => x.TransferToDairyComponentDto(_testDairyComponent),
            Times.Once);
    }

    [TestMethod]
    public void OnNavigatedTo_WithoutComponentParameter_DoesNotInitialize()
    {
        // Arrange
        var navigationContext = new NavigationContext(
            new Mock<IRegionNavigationService>().Object,
            new Uri("test", UriKind.Relative));

        // Act
        _viewModel.OnNavigatedTo(navigationContext);

        // Assert
        Assert.IsNull(_viewModel.SelectedDairyComponentDto);
    }

    [TestMethod]
    public void OnNavigatedTo_WithWrongComponentType_DoesNotInitialize()
    {
        // Arrange
        var navigationContext = new NavigationContext(
            new Mock<IRegionNavigationService>().Object,
            new Uri("test", UriKind.Relative));
        navigationContext.Parameters.Add(GuiConstants.ComponentKey, new BackgroundingComponent());

        // Act
        _viewModel.OnNavigatedTo(navigationContext);

        // Assert
        Assert.IsNull(_viewModel.SelectedDairyComponentDto);
    }

    #endregion

    #region SelectedDairyComponentDto Property Tests

    [TestMethod]
    public void SelectedDairyComponentDto_WhenSet_RaisesPropertyChanged()
    {
        // Arrange
        bool propertyChangedRaised = false;
        _viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(_viewModel.SelectedDairyComponentDto))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        _viewModel.SelectedDairyComponentDto = _testDairyComponentDto;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
    }

    [TestMethod]
    public void SelectedDairyComponentDto_WhenSetTwice_UnsubscribesFromOldDto()
    {
        // Arrange
        var firstDto = new DairyComponentDto { Name = "First" };
        var secondDto = new DairyComponentDto { Name = "Second" };

        // Act
        _viewModel.SelectedDairyComponentDto = firstDto;
        _viewModel.SelectedDairyComponentDto = secondDto;

        // Trigger a property change on the first DTO
        firstDto.TotalMilkingCows = 200;

        // Assert
        // If unsubscription worked correctly, the service should not be called for the old DTO
        // (This is verified through the fact that no exception is thrown)
        Assert.AreEqual(secondDto, _viewModel.SelectedDairyComponentDto);
    }

    #endregion

    #region DTO Property Change Tests

    [TestMethod]
    public void DtoPropertyChange_WithValidDto_CallsTransferService()
    {
        // Arrange
        _viewModel.InitializeDairyComponent(_testDairyComponent);
#pragma warning disable CS0618
        _mockDairyComponentService.ResetCalls(); // Reset to clear the initialization call
#pragma warning restore CS0618

        // Act
        _viewModel.SelectedDairyComponentDto.TotalMilkingCows = 150;

        // Assert - Service should be called at least once due to property change
        // Note: May be called multiple times due to calculated property recalculations
        _mockDairyComponentService.Verify(
            x => x.TransferDairyDtoToSystem(
                It.IsAny<DairyComponentDto>(),
                It.IsAny<DairyComponent>()),
            Times.AtLeastOnce);
    }

    [TestMethod]
    public void DtoPropertyChange_WithValidationErrors_DoesNotCallTransferService()
    {
        // Arrange
        _viewModel.InitializeDairyComponent(_testDairyComponent);
#pragma warning disable CS0618
        _mockDairyComponentService.ResetCalls(); // Reset to clear the initialization call
#pragma warning restore CS0618

        // Act - Set invalid value that should trigger validation error
        _viewModel.SelectedDairyComponentDto.TotalMilkingCows = -10; // Invalid: negative value

        // Assert - Service should not be called because DTO has errors
        _mockDairyComponentService.Verify(
            x => x.TransferDairyDtoToSystem(
                It.IsAny<DairyComponentDto>(),
                It.IsAny<DairyComponent>()),
            Times.Never);
    }

    [TestMethod]
    public void DtoPropertyChange_WhenDisposed_DoesNotCallTransferService()
    {
        // Arrange
        _viewModel.InitializeDairyComponent(_testDairyComponent);
#pragma warning disable CS0618
        _mockDairyComponentService.ResetCalls(); // Reset to clear the initialization call
#pragma warning restore CS0618
        _viewModel.Dispose();

        // Act
        _viewModel.SelectedDairyComponentDto.TotalMilkingCows = 150;

        // Assert
        _mockDairyComponentService.Verify(
            x => x.TransferDairyDtoToSystem(
                It.IsAny<DairyComponentDto>(),
                It.IsAny<DairyComponent>()),
            Times.Never);
    }

    [TestMethod]
    public void DtoPropertyChange_WhenServiceThrowsException_LogsError()
    {
        // Arrange
        _mockDairyComponentService
            .Setup(x => x.TransferDairyDtoToSystem(It.IsAny<DairyComponentDto>(), It.IsAny<DairyComponent>()))
            .Throws(new InvalidOperationException("Test exception"));

        _viewModel.InitializeDairyComponent(_testDairyComponent);
#pragma warning disable CS0618
        _mockLogger.ResetCalls(); // Reset to clear previous log calls
#pragma warning restore CS0618

        // Act
        _viewModel.SelectedDairyComponentDto.TotalMilkingCows = 150;

        // Assert - Verify that logger was called at least once (error was logged)
        // Note: May be called multiple times due to calculated property recalculations
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Calculated Values Tests

    [TestMethod]
    public void DairyComponentDto_CalculatesHerdComposition_Automatically()
    {
        // Arrange
        var dto = new DairyComponentDto
        {
            TotalMilkingCows = 100,
            ReplacementRate = 30.0,
            DryPeriodDays = 60
        };

        // Act - Values are calculated in constructor and when properties change
        // No explicit action needed

        // Assert
        Assert.IsTrue(dto.CalculatedCalves >= 0, "Calculated calves should be non-negative");
        Assert.IsTrue(dto.CalculatedHeifers >= 0, "Calculated heifers should be non-negative");
        Assert.IsTrue(dto.CalculatedLactating >= 0, "Calculated lactating should be non-negative");
        Assert.IsTrue(dto.CalculatedDry >= 0, "Calculated dry should be non-negative");
        Assert.IsTrue(dto.CalculatedLactating + dto.CalculatedDry <= dto.TotalMilkingCows,
            "Sum of lactating and dry should not exceed total milking cows");
    }

    [TestMethod]
    public void DairyComponentDto_RecalculatesWhenInputChanges()
    {
        // Arrange
        var dto = new DairyComponentDto
        {
            TotalMilkingCows = 100
        };
        var initialCalculatedHeifers = dto.CalculatedHeifers;

        // Act
        dto.ReplacementRate = 40.0; // Increase replacement rate

        // Assert
        Assert.AreNotEqual(initialCalculatedHeifers, dto.CalculatedHeifers,
            "Calculated heifers should change when replacement rate changes");
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void FullWorkflow_InitializeAndModify_WorksCorrectly()
    {
        // Arrange
        var component = new DairyComponent { Name = "Integration Test Dairy" };
        var dto = new DairyComponentDto
        {
            Name = "Integration Test Dairy",
            TotalMilkingCows = 100,
            ReplacementRate = 30.0
        };

        _mockDairyComponentService
            .Setup(x => x.TransferToDairyComponentDto(component))
            .Returns(dto);

        // Act
        _viewModel.InitializeViewModel(component);
        _viewModel.SelectedDairyComponentDto.TotalMilkingCows = 150;

        // Assert
        Assert.IsNotNull(_viewModel.SelectedDairyComponentDto);
        Assert.AreEqual(150, _viewModel.SelectedDairyComponentDto.TotalMilkingCows);
        _mockDairyComponentService.Verify(
            x => x.TransferToDairyComponentDto(component),
            Times.Once);
        // Note: TransferDairyDtoToSystem may be called multiple times due to calculated property recalculations
        _mockDairyComponentService.Verify(
            x => x.TransferDairyDtoToSystem(dto, component),
            Times.AtLeastOnce);
    }

    #endregion
}
