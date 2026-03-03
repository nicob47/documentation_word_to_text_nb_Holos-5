using H.Core.Factories;
using H.Core.Factories.Crops;
using H.Core.Factories.Fields;
using H.Core.Factories.Rotations;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
using H.Core.Models.LandManagement.Rotation;
using H.Core.Services;
using H.Core.Services.Animals;
using H.Core.Services.LandManagement.Fields;
using Microsoft.Extensions.Logging;
using Moq;

#nullable disable

namespace H.Core.Test.Services.LandManagement;

[TestClass]
public class RotationComponentServiceTest
{
    #region Fields

    private IRotationComponentService _rotationComponentService = null!;
    private Mock<ILogger> _mockLogger = null!;
    private Mock<IFieldFactory> _mockFieldFactory = null!;
    private Mock<ICropFactory> _mockCropFactory = null!;
    private Mock<ITransferService<RotationComponent, RotationComponentDto>> _mockRotationTransferService = null!;

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
        _mockLogger = new Mock<ILogger>();
        _mockFieldFactory = new Mock<IFieldFactory>();
        _mockCropFactory = new Mock<ICropFactory>();
        _mockRotationTransferService = new Mock<ITransferService<RotationComponent, RotationComponentDto>>();
        _rotationComponentService = new RotationComponentService(
            _mockLogger.Object, 
            _mockFieldFactory.Object, 
            _mockCropFactory.Object,
            _mockRotationTransferService.Object);
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
        // Arrange: valid dependencies
        var mockLogger = new Mock<ILogger>();
        var mockFieldFactory = new Mock<IFieldFactory>();
        var mockCropFactory = new Mock<ICropFactory>();
        var mockRotationTransferService = new Mock<ITransferService<RotationComponent, RotationComponentDto>>();

        // Act: create instance with valid dependencies
        var service = new RotationComponentService(
            mockLogger.Object, 
            mockFieldFactory.Object, 
            mockCropFactory.Object,
            mockRotationTransferService.Object);

        // Assert: instance should be created successfully
        Assert.IsNotNull(service);
        Assert.IsInstanceOfType(service, typeof(RotationComponentService));
        Assert.IsInstanceOfType(service, typeof(IRotationComponentService));
    }

    [TestMethod]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange: null logger parameter
        ILogger logger = null;
        var mockFieldFactory = new Mock<IFieldFactory>();
        var mockCropFactory = new Mock<ICropFactory>();
        var mockRotationTransferService = new Mock<ITransferService<RotationComponent, RotationComponentDto>>();

        // Act & Assert: attempt to create instance with null logger
        Assert.ThrowsExactly<ArgumentNullException>(() => 
            new RotationComponentService(logger, mockFieldFactory.Object, mockCropFactory.Object, mockRotationTransferService.Object));
    }

    [TestMethod]
    public void Constructor_WithNullFieldFactory_ThrowsArgumentNullException()
    {
        // Arrange: null field factory parameter
        var mockLogger = new Mock<ILogger>();
        IFieldFactory fieldFactory = null;
        var mockCropFactory = new Mock<ICropFactory>();
        var mockRotationTransferService = new Mock<ITransferService<RotationComponent, RotationComponentDto>>();

        // Act & Assert: attempt to create instance with null field factory
        Assert.ThrowsExactly<ArgumentNullException>(() => 
            new RotationComponentService(mockLogger.Object, fieldFactory, mockCropFactory.Object, mockRotationTransferService.Object));
    }

    [TestMethod]
    public void Constructor_WithNullCropFactory_ThrowsArgumentNullException()
    {
        // Arrange: null crop factory parameter
        var mockLogger = new Mock<ILogger>();
        var mockFieldFactory = new Mock<IFieldFactory>();
        ICropFactory cropFactory = null;
        var mockRotationTransferService = new Mock<ITransferService<RotationComponent, RotationComponentDto>>();

        // Act & Assert: attempt to create instance with null crop factory
        Assert.ThrowsExactly<ArgumentNullException>(() => 
            new RotationComponentService(mockLogger.Object, mockFieldFactory.Object, cropFactory, mockRotationTransferService.Object));
    }

    [TestMethod]
    public void Constructor_WithNullRotationTransferService_ThrowsArgumentNullException()
    {
        // Arrange: null rotation transfer service parameter
        var mockLogger = new Mock<ILogger>();
        var mockFieldFactory = new Mock<IFieldFactory>();
        var mockCropFactory = new Mock<ICropFactory>();
        ITransferService<RotationComponent, RotationComponentDto> rotationTransferService = null;

        // Act & Assert: attempt to create instance with null rotation transfer service
        Assert.ThrowsExactly<ArgumentNullException>(() => 
            new RotationComponentService(mockLogger.Object, mockFieldFactory.Object, mockCropFactory.Object, rotationTransferService));
    }

    [TestMethod]
    public void Constructor_VerifyParameterNames_InExceptionMessages()
    {
        // Test logger parameter name in exception
        try
        {
            new RotationComponentService(null, new Mock<IFieldFactory>().Object, new Mock<ICropFactory>().Object, new Mock<ITransferService<RotationComponent, RotationComponentDto>>().Object);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException ex)
        {
            Assert.AreEqual("logger", ex.ParamName);
        }

        // Test fieldFactory parameter name in exception
        try
        {
            new RotationComponentService(new Mock<ILogger>().Object, null, new Mock<ICropFactory>().Object, new Mock<ITransferService<RotationComponent, RotationComponentDto>>().Object);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException ex)
        {
            Assert.AreEqual("fieldFactory", ex.ParamName);
        }

        // Test cropFactory parameter name in exception
        try
        {
            new RotationComponentService(new Mock<ILogger>().Object, new Mock<IFieldFactory>().Object, null, new Mock<ITransferService<RotationComponent, RotationComponentDto>>().Object);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException ex)
        {
            Assert.AreEqual("cropFactory", ex.ParamName);
        }

        // Test rotationTransferService parameter name in exception
        try
        {
            new RotationComponentService(new Mock<ILogger>().Object, new Mock<IFieldFactory>().Object, new Mock<ICropFactory>().Object, null);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException ex)
        {
            Assert.AreEqual("rotationTransferService", ex.ParamName);
        }
    }

    #endregion

    #region InitializeComponent Tests

    [TestMethod]
    public void InitializeComponent_WithNullRotationComponent_LogsErrorAndReturns()
    {
        // Arrange: valid farm but null rotation component
        var farm = new Farm();
        RotationComponent rotationComponent = null;

        // Act: should log error and return early when rotationComponent is null
        _rotationComponentService.InitializeComponent(farm, rotationComponent);

        // Assert: verify error logging occurred
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Called with null component parameter")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void InitializeComponent_WithValidRotationComponent_SetsNameAndInitializesFlag()
    {
        // Arrange: valid farm and uninitialized rotation component
        var farm = new Farm();
        var rotationComponent = new RotationComponent
        {
            Name = null,
            IsInitialized = false,
            ComponentNameDisplayString = "Crop Rotation"
        };

        // Act: initialize the component
        _rotationComponentService.InitializeComponent(farm, rotationComponent);

        // Assert: component should now be initialized with a unique name
        Assert.IsTrue(rotationComponent.IsInitialized);
        Assert.IsNotNull(rotationComponent.Name);
        Assert.AreEqual("Crop Rotation", rotationComponent.Name);
    }

    [TestMethod]
    public void InitializeComponent_WithAlreadyInitializedComponent_LogsDebugAndReturns()
    {
        // Arrange: rotation component that is already initialized
        var farm = new Farm();
        var originalName = "Original Rotation Name";
        var rotationComponent = new RotationComponent
        {
            IsInitialized = true,
            Name = originalName
        };

        // Act: attempt to initialize an already initialized component
        _rotationComponentService.InitializeComponent(farm, rotationComponent);

        // Assert: component should remain unchanged and debug message should be logged
        Assert.IsTrue(rotationComponent.IsInitialized);
        Assert.AreEqual(originalName, rotationComponent.Name);

        // Verify debug logging occurred
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Component") && v.ToString().Contains("is already initialized")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void InitializeComponent_WithFarmContainingNoComponents_AssignsBaseName()
    {
        // Arrange: empty farm with no existing components
        var farm = new Farm() { Components = new List<ComponentBase>() };
        var rotationComponent = new RotationComponent
        {
            ComponentNameDisplayString = "Test Rotation",
            IsInitialized = false
        };

        // Act
        _rotationComponentService.InitializeComponent(farm, rotationComponent);

        // Assert: should get the base name without any numbering
        Assert.IsTrue(rotationComponent.IsInitialized);
        Assert.AreEqual("Test Rotation", rotationComponent.Name);
    }

    [TestMethod]
    public void InitializeComponent_WithFarmContainingExistingComponents_AssignsUniqueName()
    {
        // Arrange: farm with existing components that have conflicting names
        var existingComponent1 = new RotationComponent() { Name = "Crop Rotation" };
        var existingComponent2 = new RotationComponent() { Name = "Crop Rotation #2" };
        var farm = new Farm()
        {
            Components = new List<ComponentBase> { existingComponent1, existingComponent2 }
        };

        var newRotationComponent = new RotationComponent
        {
            ComponentNameDisplayString = "Crop Rotation",
            IsInitialized = false
        };

        // Act
        _rotationComponentService.InitializeComponent(farm, newRotationComponent);

        // Assert: should get a unique name that doesn't conflict with existing components
        Assert.IsTrue(newRotationComponent.IsInitialized);
        Assert.AreEqual("Crop Rotation #3", newRotationComponent.Name);
    }

    [TestMethod]
    public void InitializeComponent_WithMultipleCallsOnSameComponent_OnlyInitializesOnce()
    {
        // Arrange: single rotation component and farm
        var farm = new Farm() { Components = new List<ComponentBase>() };
        var rotationComponent = new RotationComponent
        {
            ComponentNameDisplayString = "Rotation",
            IsInitialized = false
        };

        // Act: call initialize multiple times
        _rotationComponentService.InitializeComponent(farm, rotationComponent);
        var nameAfterFirstCall = rotationComponent.Name;

        _rotationComponentService.InitializeComponent(farm, rotationComponent);
        _rotationComponentService.InitializeComponent(farm, rotationComponent);

        // Assert: should only be initialized once and name should not change
        Assert.IsTrue(rotationComponent.IsInitialized);
        Assert.AreEqual(nameAfterFirstCall, rotationComponent.Name);
    }

    [TestMethod]
    public void InitializeComponent_WithNullFarm_HandlesGracefully()
    {
        // Arrange: null farm parameter
        Farm farm = null;
        var rotationComponent = new RotationComponent
        {
            ComponentNameDisplayString = "Rotation",
            IsInitialized = false
        };

        // Act: should handle null farm gracefully (base class behavior)
        _rotationComponentService.InitializeComponent(farm, rotationComponent);

        // Assert: component should still be marked as initialized
        Assert.IsTrue(rotationComponent.IsInitialized);
        Assert.IsNotNull(rotationComponent.Name);
        // Name should include timestamp since farm is null
        Assert.IsTrue(rotationComponent.Name.Contains("Rotation_"));
    }

    [TestMethod]
    public void InitializeComponent_WithComplexRotationComponent_InitializesCorrectly()
    {
        // Arrange: rotation component with complex properties
        var farm = new Farm();
        var rotationComponent = new RotationComponent
        {
            ComponentNameDisplayString = "Complex Rotation System",
            ComponentType = ComponentType.Rotation,
            ShiftLeft = true,
            KeepRotationOnSingleField = false,
            IsInitialized = false
        };

        // Add some field components to make it more realistic
        rotationComponent.FieldSystemComponents.Add(new FieldSystemComponent
        {
            Name = "Field 1",
            FieldArea = 100.5
        });
        rotationComponent.FieldSystemComponents.Add(new FieldSystemComponent
        {
            Name = "Field 2",
            FieldArea = 200.75
        });

        // Act: method should initialize correctly regardless of component complexity
        _rotationComponentService.InitializeComponent(farm, rotationComponent);

        // Assert: component should be properly initialized
        Assert.IsTrue(rotationComponent.IsInitialized);
        Assert.AreEqual("Complex Rotation System", rotationComponent.Name);
        Assert.AreEqual(ComponentType.Rotation, rotationComponent.ComponentType);
        Assert.IsTrue(rotationComponent.ShiftLeft);
        Assert.IsFalse(rotationComponent.KeepRotationOnSingleField);
    }

    [TestMethod]
    public void InitializeComponent_WithComponentsHavingEmptyNames_IgnoresEmptyNames()
    {
        // Arrange: farm with existing components where some have empty/null names
        var componentWithEmptyName = new RotationComponent() { Name = "" };
        var componentWithNullName = new RotationComponent() { Name = null };
        var componentWithWhitespaceName = new RotationComponent() { Name = "   " };
        var farm = new Farm()
        {
            Components = new List<ComponentBase>
            {
                componentWithEmptyName,
                componentWithNullName,
                componentWithWhitespaceName
            }
        };

        var newRotationComponent = new RotationComponent
        {
            ComponentNameDisplayString = "Rotation",
            IsInitialized = false
        };

        // Act: empty/null names should be ignored in uniqueness check
        _rotationComponentService.InitializeComponent(farm, newRotationComponent);

        // Assert: should get base name since empty names are ignored
        Assert.IsTrue(newRotationComponent.IsInitialized);
        Assert.AreEqual("Rotation", newRotationComponent.Name);
    }

    [TestMethod]
    public void InitializeComponent_LogsSuccessfulInitialization()
    {
        // Arrange: valid farm and uninitialized rotation component
        var farm = new Farm();
        var rotationComponent = new RotationComponent
        {
            ComponentNameDisplayString = "Test Rotation",
            IsInitialized = false
        };

        // Act: initialize the component
        _rotationComponentService.InitializeComponent(farm, rotationComponent);

        // Assert: verify all expected log messages occurred
        // Debug message for starting initialization
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Initializing component:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        // Info message for successful completion
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully initialized component with name:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region Interface Implementation Tests

    [TestMethod]
    public void RotationComponentService_ImplementsIRotationComponentService()
    {
        // Act: verify service implements the expected interface
        var implementsInterface = _rotationComponentService is IRotationComponentService;

        // Assert: service should implement the interface
        Assert.IsTrue(implementsInterface);
        Assert.IsInstanceOfType(_rotationComponentService, typeof(IRotationComponentService));
    }

    [TestMethod]
    public void RotationComponentService_InheritsFromComponentServiceBase()
    {
        // Act: verify service inherits from the expected base class
        var inheritsFromBase = _rotationComponentService is ComponentServiceBase;

        // Assert: service should inherit from ComponentServiceBase
        Assert.IsTrue(inheritsFromBase);
        Assert.IsInstanceOfType(_rotationComponentService, typeof(ComponentServiceBase));
    }

    [TestMethod]
    public void IRotationComponentService_HasExpectedMethod()
    {
        // Arrange: get the interface type
        var interfaceType = typeof(IRotationComponentService);
        
        // Act: check for the expected method
        var method = interfaceType.GetMethod("InitializeComponent");
        
        // Assert: method should exist with correct signature
        Assert.IsNotNull(method, "InitializeComponent method should exist on interface");
        
        var parameters = method.GetParameters();
        Assert.AreEqual(2, parameters.Length, "Method should have exactly 2 parameters");
        Assert.AreEqual(typeof(Farm), parameters[0].ParameterType, "First parameter should be Farm type");
        Assert.AreEqual("farm", parameters[0].Name, "First parameter should be named 'farm'");
        Assert.AreEqual(typeof(RotationComponent), parameters[1].ParameterType, "Second parameter should be RotationComponent type");
        Assert.AreEqual("rotationComponent", parameters[1].Name, "Second parameter should be named 'rotationComponent'");
        Assert.AreEqual(typeof(void), method.ReturnType, "Method should return void");
    }

    #endregion
}
