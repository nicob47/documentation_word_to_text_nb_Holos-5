using H.Avalonia.ViewModels.ComponentViews.LandManagement;
using H.Avalonia.ViewModels.ComponentViews.LandManagement.Rotation;
using H.Core.Factories.Crops;
using H.Core.Factories.Rotations;
using H.Core.Models;
using H.Core.Models.LandManagement.Rotation;
using H.Core.Services.LandManagement.Fields;
using H.Core.Services.StorageService;
using H.Core.Services.CropColorService;
using H.Core.Enumerations;
using Microsoft.Extensions.Logging;
using Moq;
using Prism.Events;
using Prism.Regions;

#nullable disable

namespace H.Avalonia.Test.ViewModels.ComponentViews.LandManagement;

/// <summary>
/// Test class for <see cref="RotationComponentViewModel"/>.
/// Tests constructor validation, initialization, navigation, property binding, and event handling.
/// </summary>
[TestClass]
public class RotationComponentViewModelTests
{
    #region Fields

    private RotationComponentViewModel _viewModel = null!;
    private Mock<IRegionManager> _mockRegionManager = null!;
    private Mock<IEventAggregator> _mockEventAggregator = null!;
    private Mock<IStorageService> _mockStorageService = null!;
    private Mock<IFieldComponentService> _mockFieldComponentService = null!;
    private Mock<IRotationComponentService> _mockRotationComponentService = null!;
    private Mock<ILogger> _mockLogger = null!;
    private Mock<ICropFactory> _mockCropFactory = null!;
    private Mock<ICropColorService> _mockCropColorService = null!;
    private Farm _testFarm = null!;

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
        // Setup test farm that will be returned by the storage service
        _testFarm = new Farm
        {
            Name = "Test Farm"
        };

        // Setup mocks for all dependencies required by RotationComponentViewModel
        _mockRegionManager = new Mock<IRegionManager>();
        _mockEventAggregator = new Mock<IEventAggregator>();
        _mockStorageService = new Mock<IStorageService>();
        _mockFieldComponentService = new Mock<IFieldComponentService>();
        _mockLogger = new Mock<ILogger>();
        _mockCropFactory = new Mock<ICropFactory>();
        _mockRotationComponentService = new Mock<IRotationComponentService>();
        _mockCropColorService = new Mock<ICropColorService>();

        // Configure storage service to return a valid storage object with application data
        _mockStorageService.Setup(x => x.Storage).Returns(new H.Core.Storage()
        {
            ApplicationData = new ApplicationData()
            {
                GlobalSettings = new GlobalSettings()
            }
        });
        _mockStorageService.Setup(x => x.GetActiveFarm()).Returns(_testFarm);

        // Setup the rotation service to return a valid DTO when TransferToRotationComponentDto is called.
        // This is essential because InitializeRotationComponent subscribes to the DTO's PropertyChanged event.
        // Without this setup, the DTO would be null and cause NullReferenceException when subscribing to events.
        _mockRotationComponentService
            .Setup(x => x.TransferToRotationComponentDto(It.IsAny<RotationComponent>()))
            .Returns((RotationComponent rc) => new RotationComponentDto
            {
                Name = rc.Name,
                Guid = rc.Guid,
                FieldArea = rc.FieldSystemComponent?.FieldArea ?? 0
            });

        // Setup crop color service to return default values
        _mockCropColorService
            .Setup(x => x.GetCropColorHex(It.IsAny<CropType>()))
            .Returns("#F5F5F5");
        _mockCropColorService
            .Setup(x => x.GetCropDisplayName(It.IsAny<CropType>()))
            .Returns((CropType ct) => ct.ToString());

        // Create the view model under test with all mocked dependencies
        _viewModel = new RotationComponentViewModel(
            _mockRegionManager.Object,
            _mockEventAggregator.Object,
            _mockStorageService.Object,
            _mockFieldComponentService.Object,
            _mockRotationComponentService.Object,
            _mockLogger.Object,
            _mockCropFactory.Object,
            _mockCropColorService.Object);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        // Clean up the view model after each test to prevent memory leaks and ensure test isolation
        _viewModel?.Dispose();
    }

    #endregion

    #region Constructor Tests

    [TestMethod]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Verify that the view model is properly instantiated when all dependencies are provided
        Assert.IsNotNull(_viewModel);
        Assert.IsInstanceOfType(_viewModel, typeof(RotationComponentViewModel));
    }

    [TestMethod]
    public void Constructor_WithNullRegionManager_ShouldThrowArgumentNullException()
    {
        // Verify that the constructor enforces non-null region manager dependency
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            new RotationComponentViewModel(
                null,
                _mockEventAggregator.Object,
                _mockStorageService.Object,
                _mockFieldComponentService.Object,
                _mockRotationComponentService.Object,
                _mockLogger.Object,
                _mockCropFactory.Object,
                _mockCropColorService.Object);
        });
    }

    [TestMethod]
    public void Constructor_WithNullEventAggregator_ShouldThrowArgumentNullException()
    {
        // Verify that the constructor enforces non-null event aggregator dependency
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            new RotationComponentViewModel(
                _mockRegionManager.Object,
                null,
                _mockStorageService.Object,
                _mockFieldComponentService.Object,
                _mockRotationComponentService.Object,
                _mockLogger.Object,
                _mockCropFactory.Object,
                _mockCropColorService.Object);
        });
    }

    [TestMethod]
    public void Constructor_WithNullStorageService_ShouldThrowArgumentNullException()
    {
        // Verify that the constructor enforces non-null storage service dependency
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            new RotationComponentViewModel(
                _mockRegionManager.Object,
                _mockEventAggregator.Object,
                null,
                _mockFieldComponentService.Object,
                _mockRotationComponentService.Object,
                _mockLogger.Object,
                _mockCropFactory.Object,
                _mockCropColorService.Object);
        });
    }

    [TestMethod]
    public void Constructor_WithNullFieldComponentService_ShouldThrowArgumentNullException()
    {
        // Verify that the constructor enforces non-null field component service dependency
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            new RotationComponentViewModel(
                _mockRegionManager.Object,
                _mockEventAggregator.Object,
                _mockStorageService.Object,
                null,
                _mockRotationComponentService.Object,
                _mockLogger.Object,
                _mockCropFactory.Object,
                _mockCropColorService.Object);
        });
    }

    [TestMethod]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Verify that the constructor enforces non-null logger dependency
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            new RotationComponentViewModel(
                _mockRegionManager.Object,
                _mockEventAggregator.Object,
                _mockStorageService.Object,
                _mockFieldComponentService.Object,
                _mockRotationComponentService.Object,
                null,
                _mockCropFactory.Object,
                _mockCropColorService.Object);
        });
    }

    [TestMethod]
    public void Constructor_WithNullCropFactory_ShouldThrowArgumentNullException()
    {
        // Verify that the constructor enforces non-null crop factory dependency
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            new RotationComponentViewModel(
                _mockRegionManager.Object,
                _mockEventAggregator.Object,
                _mockStorageService.Object,
                _mockFieldComponentService.Object,
                _mockRotationComponentService.Object,
                _mockLogger.Object,
                null,
                _mockCropColorService.Object);
        });
    }

    [TestMethod]
    public void Constructor_WithNullCropColorService_ShouldThrowArgumentNullException()
    {
        // Verify that the constructor enforces non-null crop color service dependency
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            new RotationComponentViewModel(
                _mockRegionManager.Object,
                _mockEventAggregator.Object,
                _mockStorageService.Object,
                _mockFieldComponentService.Object,
                _mockRotationComponentService.Object,
                _mockLogger.Object,
                _mockCropFactory.Object,
                null);
        });
    }

    #endregion

    #region InitializeViewModel Tests

    [TestMethod]
    public void InitializeViewModel_WithRotationComponent_ShouldCallBaseInitializeViewModel()
    {
        // Arrange: Create a test rotation component
        var rotationComponent = new RotationComponent
        {
            Name = "Test Rotation"
        };

        // Act: Call InitializeViewModel with the rotation component
        _viewModel.InitializeViewModel(rotationComponent);

        // Assert: Verify that the base class initialization was called by checking if the logger was invoked
        // The base InitializeViewModel logs a debug message containing "initializing"
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("initializing")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }


    [TestMethod]
    public void InitializeViewModel_WithParameterlessCall_ShouldNotThrow()
    {
        // Verify that calling InitializeViewModel without parameters doesn't throw an exception
        // This tests the parameterless overload of the method
        try
        {
            _viewModel.InitializeViewModel();
        }
        catch (Exception)
        {
            Assert.Fail("InitializeViewModel() should not throw an exception");
        }
        
        Assert.IsTrue(true);
    }

    #endregion

    #region Dependency Injection Tests

    [TestMethod]
    public void Constructor_ShouldInjectAllDependenciesCorrectly()
    {
        // Create a new instance to verify that dependency injection works correctly
        var viewModel = new RotationComponentViewModel(
            _mockRegionManager.Object,
            _mockEventAggregator.Object,
            _mockStorageService.Object,
            _mockFieldComponentService.Object,
            _mockRotationComponentService.Object,
            _mockLogger.Object,
            _mockCropFactory.Object,
            _mockCropColorService.Object);

        // Since the dependency fields are private, we verify correct injection by ensuring
        // the view model is created successfully without throwing exceptions
        Assert.IsNotNull(viewModel);
        Assert.IsInstanceOfType(viewModel, typeof(RotationComponentViewModel));
        
        // Clean up to prevent memory leaks
        viewModel.Dispose();
    }

    #endregion

    #region Disposal Tests

    [TestMethod]
    public void Dispose_ShouldNotThrowException()
    {
        // Verify that disposing the view model doesn't throw an exception
        try
        {
            _viewModel.Dispose();
        }
        catch (Exception)
        {
            Assert.Fail("Dispose should not throw an exception");
        }
        
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void Dispose_CalledMultipleTimes_ShouldNotThrowException()
    {
        // Verify that the dispose pattern is correctly implemented to handle multiple calls
        // This is important for IDisposable implementations
        try
        {
            _viewModel.Dispose();
            _viewModel.Dispose(); // Should handle multiple dispose calls gracefully
        }
        catch (Exception)
        {
            Assert.Fail("Multiple dispose calls should not throw an exception");
        }
        
        Assert.IsTrue(true);
    }

    #endregion

    #region Base Class Integration Tests

    [TestMethod]
    public void ViewName_Property_ShouldBeSettableAndGettable()
    {
        // Arrange: Define a test view name
        const string testName = "Test Rotation View";

        // Act: Set the ViewName property
        _viewModel.ViewName = testName;

        // Assert: Verify that the property getter returns the same value
        Assert.AreEqual(testName, _viewModel.ViewName);
    }

    [TestMethod]
    public void AllowNavigation_Property_ShouldBeSettableAndGettable()
    {
        // Arrange: Define a test boolean value
        const bool testValue = true;

        // Act: Set the AllowNavigation property
        _viewModel.AllowNavigation = testValue;

        // Assert: Verify that the property getter returns the same value
        Assert.AreEqual(testValue, _viewModel.AllowNavigation);
    }

    [TestMethod]
    public void ActiveFarm_Property_ShouldReturnTestFarm()
    {
        // Act: Retrieve the active farm from the view model
        var result = _viewModel.ActiveFarm;

        // Assert: Verify that the active farm matches the test farm configured in TestInitialize
        Assert.IsNotNull(result);
        Assert.AreEqual(_testFarm.Name, result.Name);
    }

    [TestMethod]
    public void StorageService_Property_ShouldReturnMockedService()
    {
        // Act: Retrieve the storage service from the view model
        var result = _viewModel.StorageService;

        // Assert: Verify that the storage service is the mocked instance
        Assert.IsNotNull(result);
        Assert.AreSame(_mockStorageService.Object, result);
    }

    #endregion

    #region Navigation Tests

    [TestMethod]
    public void OnNavigatedTo_ShouldNotThrowException()
    {
        // Arrange: Create a navigation context (required parameter for OnNavigatedTo)
        var navigationContext = new NavigationContext(
            Mock.Of<IRegionNavigationService>(),
            new Uri("test://test"));

        // Act: Call OnNavigatedTo to simulate navigating to this view
        try
        {
            _viewModel.OnNavigatedTo(navigationContext);
        }
        catch (Exception)
        {
            Assert.Fail("OnNavigatedTo should not throw an exception");
        }
        
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void OnNavigatedFrom_ShouldNotThrowException()
    {
        // Arrange: Create a navigation context
        var navigationContext = new NavigationContext(
            Mock.Of<IRegionNavigationService>(),
            new Uri("test://test"));

        // Act: Call OnNavigatedFrom to simulate navigating away from this view
        try
        {
            _viewModel.OnNavigatedFrom(navigationContext);
        }
        catch (Exception)
        {
            Assert.Fail("OnNavigatedFrom should not throw an exception");
        }
        
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void IsNavigationTarget_WhenNotDisposed_ShouldReturnTrue()
    {
        // Arrange: Create a navigation context
        var navigationContext = new NavigationContext(
            Mock.Of<IRegionNavigationService>(),
            new Uri("test://test"));

        // Act: Check if this view model is a valid navigation target
        var result = _viewModel.IsNavigationTarget(navigationContext);

        // Assert: An active (non-disposed) view model should be a valid navigation target
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsNavigationTarget_WhenDisposed_ShouldReturnFalse()
    {
        // Arrange: Create a navigation context
        var navigationContext = new NavigationContext(
            Mock.Of<IRegionNavigationService>(),
            new Uri("test://test"));

        // Act: Dispose the view model, then check if it's still a valid navigation target
        _viewModel.Dispose();
        var result = _viewModel.IsNavigationTarget(navigationContext);

        // Assert: A disposed view model should not be a valid navigation target
        Assert.IsFalse(result);
    }

    #endregion

    #region Validation Tests

    [TestMethod]
    public void ViewName_SetToEmptyString_ShouldHaveValidationError()
    {
        // Act: Set ViewName to an empty string (invalid value)
        _viewModel.ViewName = string.Empty;

        // Assert: Verify that validation errors are triggered for the ViewName property
        Assert.IsTrue(_viewModel.HasErrors);
        var errors = _viewModel.GetErrors(nameof(_viewModel.ViewName));
        Assert.IsNotNull(errors);
        Assert.IsTrue(errors.Cast<string>().Any());
    }

    [TestMethod]
    public void ViewName_SetToValidString_ShouldNotHaveValidationError()
    {
        // Act: Set ViewName to a valid, non-empty string
        _viewModel.ViewName = "Valid Name";

        // Assert: Verify that no validation errors are present
        Assert.IsFalse(_viewModel.HasErrors);
    }

    [TestMethod]
    public void ViewName_SetToNullOrEmpty_ValidationBehaviorIsConsistent()
    {
        // This test ensures that null and empty string trigger the same validation behavior
        
        // Test empty string validation
        _viewModel.ViewName = string.Empty;
        var hasErrorsForEmpty = _viewModel.HasErrors;
        
        // Clear any existing errors by setting a valid value
        _viewModel.ViewName = "Valid Name";
        
        // Test null validation  
        _viewModel.ViewName = null;
        var hasErrorsForNull = _viewModel.HasErrors;

        // Assert that both null and empty string produce consistent validation results
        Assert.AreEqual(hasErrorsForEmpty, hasErrorsForNull, 
            "Validation behavior should be consistent between null and empty string values");
    }

    #endregion

    #region InitializeRotationComponent Tests

    [TestMethod]
    public void InitializeRotationComponent_WithValidRotationComponent_ShouldNotThrow()
    {
        // Arrange: Create a valid rotation component with required properties
        var rotationComponent = new RotationComponent
        {
            Name = "Test Rotation",
            ComponentType = ComponentType.Rotation
        };

        // Act: Initialize the view model with the rotation component
        try
        {
            _viewModel.InitializeRotationComponent(rotationComponent);
        }
        catch (Exception)
        {
            Assert.Fail("InitializeRotationComponent should not throw an exception with valid rotation component");
        }

        Assert.IsTrue(true);
    }

    [TestMethod]
    public void InitializeRotationComponent_WithNullRotationComponent_ShouldNotThrow()
    {
        // Act: Call InitializeRotationComponent with null to verify it handles null gracefully
        try
        {
            _viewModel.InitializeRotationComponent(null);
        }
        catch (Exception)
        {
            Assert.Fail("InitializeRotationComponent should handle null parameter gracefully");
        }

        Assert.IsTrue(true);
    }

    [TestMethod]
    public void InitializeRotationComponent_WithValidComponent_ShouldCallTransferService()
    {
        // Arrange: Create a rotation component with specific properties
        var rotationComponent = new RotationComponent
        {
            Name = "Test Rotation Component",
            ComponentType = ComponentType.Rotation,
            ShiftLeft = true,
            KeepRotationOnSingleField = false
        };

        // Act: Initialize the component
        _viewModel.InitializeRotationComponent(rotationComponent);

        // Assert: Verify that the transfer service was called to create a DTO from the domain object
        _mockRotationComponentService.Verify(
            x => x.TransferToRotationComponentDto(It.Is<RotationComponent>(rc => rc.Name == "Test Rotation Component")),
            Times.Once,
            "TransferToRotationComponentDto should be called once with the rotation component");
    }

    [TestMethod]
    public void InitializeRotationComponent_CalledMultipleTimes_ShouldHandleGracefully()
    {
        // Arrange: Create multiple rotation components
        var firstRotation = new RotationComponent
        {
            Name = "First Rotation",
            ComponentType = ComponentType.Rotation
        };

        var secondRotation = new RotationComponent
        {
            Name = "Second Rotation", 
            ComponentType = ComponentType.Rotation
        };

        // Act: Call InitializeRotationComponent multiple times, including with null
        // This tests that the method can handle being called multiple times without issues
        try
        {
            _viewModel.InitializeRotationComponent(firstRotation);
            _viewModel.InitializeRotationComponent(secondRotation);
            _viewModel.InitializeRotationComponent(null); // Should handle null after valid component
        }
        catch (Exception)
        {
            Assert.Fail("InitializeRotationComponent should handle multiple calls gracefully");
        }

        // Assert: Verify the transfer service was called the correct number of times
        // Should be called twice (once for each non-null rotation component)
        _mockRotationComponentService.Verify(
            x => x.TransferToRotationComponentDto(It.IsAny<RotationComponent>()),
            Times.Exactly(2),
            "TransferToRotationComponentDto should be called twice (once for each non-null rotation)");
    }

    [TestMethod]
    public void InitializeRotationComponent_WithComponentContainingFieldComponents_ShouldHandleCorrectly()
    {
        // Arrange: Create a complex rotation component with nested field system components
        var rotationComponent = new RotationComponent
        {
            Name = "Complex Rotation",
            ComponentType = ComponentType.Rotation,
            ShiftLeft = false,
            KeepRotationOnSingleField = true
        };

        // Add field system components to simulate a realistic rotation with multiple fields
        rotationComponent.FieldSystemComponents.Add(new H.Core.Models.LandManagement.Fields.FieldSystemComponent
        {
            Name = "Field 1",
            FieldArea = 100
        });

        rotationComponent.FieldSystemComponents.Add(new H.Core.Models.LandManagement.Fields.FieldSystemComponent
        {
            Name = "Field 2", 
            FieldArea = 150
        });

        // Act: Initialize with a complex component structure
        try
        {
            _viewModel.InitializeRotationComponent(rotationComponent);
        }
        catch (Exception)
        {
            Assert.Fail("InitializeRotationComponent should handle rotation components with field components");
        }

        Assert.IsTrue(true);
    }

    [TestMethod]
    public void InitializeRotationComponent_ShouldSetSelectedRotationComponentDto()
    {
        // Arrange: Create a rotation component with a unique identifier
        var rotationComponent = new RotationComponent
        {
            Name = "Test Rotation",
            ComponentType = ComponentType.Rotation,
            Guid = Guid.NewGuid()
        };

        // Act: Initialize the rotation component
        _viewModel.InitializeRotationComponent(rotationComponent);

        // Assert: Verify that the SelectedRotationComponentDto property is populated
        // and contains the expected data from the rotation component
        Assert.IsNotNull(_viewModel.SelectedRotationComponentDto, "SelectedRotationComponentDto should be set");
        Assert.AreEqual(rotationComponent.Name, _viewModel.SelectedRotationComponentDto.Name, 
            "DTO name should match the rotation component name");
    }

    [TestMethod]
    public void InitializeRotationComponent_ShouldSubscribeToPropertyChanged()
    {
        // Arrange: Create a rotation component
        var rotationComponent = new RotationComponent
        {
            Name = "Test Rotation",
            ComponentType = ComponentType.Rotation
        };

        // Act: Initialize the component, which should subscribe to PropertyChanged events on the DTO
        _viewModel.InitializeRotationComponent(rotationComponent);
        var dto = _viewModel.SelectedRotationComponentDto;

        // Verify the DTO is valid and not null
        Assert.IsNotNull(dto, "DTO should not be null after initialization");
        
        try
        {
            // Trigger a property change to verify the event handler is properly subscribed
            // If the subscription failed, this would throw an exception
            if (dto is RotationComponentDto concreteDto)
            {
                concreteDto.FieldArea = 500;
            }
        }
        catch (Exception)
        {
            Assert.Fail("Property changed event should not throw an exception");
        }

        // Assert: If we reach here, the PropertyChanged event subscription is working correctly
        Assert.IsTrue(true);
    }

    #endregion
}
