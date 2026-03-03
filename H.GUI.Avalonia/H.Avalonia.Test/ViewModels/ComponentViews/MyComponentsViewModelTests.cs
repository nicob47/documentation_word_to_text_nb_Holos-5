using H.Avalonia.Events;
using H.Avalonia.Test.TestHelpers;
using H.Avalonia.ViewModels.ComponentViews;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
using H.Core.Services;
using H.Core.Services.StorageService;
using Microsoft.Extensions.Logging;
using Moq;
using Prism.Events;
using Prism.Regions;

#nullable disable

namespace H.Avalonia.Test.ViewModels.ComponentViews;

[TestClass]
public partial class MyComponentsViewModelTests
{
    // Add this helper class at the top of your test file (or in a suitable test helpers location)

    #region Fields

    private MyComponentsViewModel _viewModel;
    private Mock<IRegionManager> _mockRegionManager;
    private Mock<IEventAggregator> _mockEventAggregator;
    private Mock<IStorageService> _mockStorageService;
    private Mock<IComponentInitializationService> _mockComponentInitializationService;
    private Farm _testFarm;
    private ComponentBase _testComponent1;
    private ComponentBase _testComponent2;

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
        // Setup mocks
        _mockRegionManager = new Mock<IRegionManager>();
        _mockEventAggregator = new Mock<IEventAggregator>();
        _mockStorageService = new Mock<IStorageService>();
        _mockComponentInitializationService = new Mock<IComponentInitializationService>();

        // Setup test farm with components
        _testFarm = new Farm();
        _testComponent1 = new FieldSystemComponent { Name = "Test Component 1" };
        _testComponent2 = new FieldSystemComponent { Name = "Test Component 2" };
        
        _testFarm.Components.Add(_testComponent1);
        _testFarm.Components.Add(_testComponent2);

        // Setup storage service to return test farm
        var storage = new H.Core.Storage() { ApplicationData = new ApplicationData() };
        storage.ApplicationData.Farms.Add(_testFarm);
        _mockStorageService.Setup(x => x.Storage).Returns(storage);
        _mockStorageService.Setup(x => x.GetActiveFarm()).Returns(_testFarm);

        // Setup region manager mocks
        var mockRegionCollection = new Mock<IRegionCollection>();
        var mockRegion = new Mock<IRegion>();

        // Setup the IRegionCollection indexer to return a mock region for any region name
        mockRegionCollection
            .Setup(rc => rc[It.IsAny<string>()])
            .Returns(mockRegion.Object);

        // Setup the Regions property on the region manager
        _mockRegionManager
            .Setup(rm => rm.Regions)
            .Returns(mockRegionCollection.Object);

        // Setup ActiveViews to return a non-null collection
        mockRegion.Setup(r => r.ActiveViews).Returns(new TestViewsCollection());

        // Setup event aggregator mocks
        var mockComponentAddedEvent = new Mock<ComponentAddedEvent>();
        var mockEditingComponentsCompletedEvent = new Mock<EditingComponentsCompletedEvent>();
        _mockEventAggregator.Setup(x => x.GetEvent<ComponentAddedEvent>()).Returns(mockComponentAddedEvent.Object);
        _mockEventAggregator.Setup(x => x.GetEvent<EditingComponentsCompletedEvent>()).Returns(mockEditingComponentsCompletedEvent.Object);
        var mockLogger = new Mock<ILogger>();

        // Create view model
        _viewModel = new MyComponentsViewModel(
            _mockRegionManager.Object,
            _mockEventAggregator.Object,
            _mockStorageService.Object,
            _mockComponentInitializationService.Object, 
            mockLogger.Object);

        // Initialize the view model to populate components
        _viewModel.InitializeViewModel();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _viewModel = null;
    }

    #endregion

    #region Tests

    [TestMethod]
    public void TestConstructor()
    {
        Assert.IsNotNull(_viewModel);
        Assert.IsNotNull(_viewModel.RemoveComponent);
        Assert.IsNotNull(_viewModel.RemoveSpecificComponentCommand);
        Assert.IsNotNull(_viewModel.MyComponents);
    }

    [TestMethod]
    public void TestInitializeViewModel_PopulatesComponents()
    {
        // Arrange - TestInitialize already calls InitializeViewModel
        
        // Assert
        Assert.AreEqual(2, _viewModel.MyComponents.Count);
        Assert.AreEqual("Test Component 1", _viewModel.MyComponents[0].Name);
        Assert.AreEqual("Test Component 2", _viewModel.MyComponents[1].Name);
    }

    [TestMethod]
    public void TestRemoveComponentCanExecute_WhenNoComponentSelected_ReturnsFalse()
    {
        // Arrange
        _viewModel.SelectedComponent = null;

        // Act
        var canExecute = _viewModel.RemoveComponent.CanExecute();

        // Assert
        Assert.IsFalse(canExecute);
    }

    [TestMethod]
    public void TestRemoveComponentCanExecute_WhenComponentSelected_ReturnsTrue()
    {
        // Arrange
        _viewModel.SelectedComponent = _testComponent1;

        // Act
        var canExecute = _viewModel.RemoveComponent.CanExecute();

        // Assert
        Assert.IsTrue(canExecute);
    }

    [TestMethod]
    public void TestRemoveComponentExecute_WhenNoComponentSelected_DoesNothing()
    {
        // Arrange
        _viewModel.SelectedComponent = null;
        var initialCount = _viewModel.MyComponents.Count;
        var initialFarmCount = _testFarm.Components.Count;

        // Act
        _viewModel.RemoveComponent.Execute();

        // Assert
        Assert.AreEqual(initialCount, _viewModel.MyComponents.Count);
        Assert.AreEqual(initialFarmCount, _testFarm.Components.Count);
    }

    [TestMethod]
    public void TestRemoveComponentExecute_WhenComponentSelected_RemovesFromBothCollections()
    {
        // Arrange
        _viewModel.SelectedComponent = _testComponent1;
        var initialMyComponentsCount = _viewModel.MyComponents.Count;
        var initialFarmComponentsCount = _testFarm.Components.Count;

        // Act
        _viewModel.RemoveComponent.Execute();

        // Assert
        Assert.AreEqual(initialMyComponentsCount - 1, _viewModel.MyComponents.Count);
        Assert.AreEqual(initialFarmComponentsCount - 1, _testFarm.Components.Count);
        Assert.IsFalse(_viewModel.MyComponents.Contains(_testComponent1));
        Assert.IsFalse(_testFarm.Components.Contains(_testComponent1));
    }

    [TestMethod]
    public void TestRemoveSpecificComponentExecute_WhenValidComponentItem_RemovesComponent()
    {
        // Arrange
        var componentItemToRemove = _viewModel.MyComponentItems.First(x => x.Component == _testComponent1);
        var initialMyComponentsCount = _viewModel.MyComponents.Count;
        var initialFarmComponentsCount = _testFarm.Components.Count;

        // Act
        _viewModel.RemoveSpecificComponentCommand.Execute(componentItemToRemove);

        // Assert
        Assert.AreEqual(initialMyComponentsCount - 1, _viewModel.MyComponents.Count);
        Assert.AreEqual(initialFarmComponentsCount - 1, _testFarm.Components.Count);
        Assert.IsFalse(_viewModel.MyComponents.Contains(_testComponent1));
        Assert.IsFalse(_testFarm.Components.Contains(_testComponent1));
    }

    [TestMethod]
    public void TestRemoveSpecificComponentExecute_WhenRemovedComponentWasSelected_SelectsAnotherComponent()
    {
        // Arrange
        _viewModel.SelectedComponent = _testComponent1;
        var componentItemToRemove = _viewModel.MyComponentItems.First(x => x.Component == _testComponent1);

        // Act
        _viewModel.RemoveSpecificComponentCommand.Execute(componentItemToRemove);

        // Assert
        Assert.AreNotEqual(_testComponent1, _viewModel.SelectedComponent);
        Assert.AreEqual(_testComponent2, _viewModel.SelectedComponent); // Should select the remaining component
    }

    [TestMethod]
    public void TestRemoveComponentExecute_AfterRemoval_SelectsLastComponent()
    {
        // Arrange
        _viewModel.SelectedComponent = _testComponent1; // Select first component

        // Act
        _viewModel.RemoveComponent.Execute();

        // Assert
        Assert.AreEqual(_testComponent2, _viewModel.SelectedComponent); // Should select the last remaining component
    }

    [TestMethod]
    public void TestRemoveComponentExecute_RemoveLastComponent_SelectsNull()
    {
        // Arrange - Remove first component first
        _viewModel.SelectedComponent = _testComponent1;
        _viewModel.RemoveComponent.Execute();
        
        // Now remove the last component
        _viewModel.SelectedComponent = _testComponent2;

        // Act
        _viewModel.RemoveComponent.Execute();

        // Assert
        Assert.AreEqual(0, _viewModel.MyComponents.Count);
        Assert.AreEqual(0, _testFarm.Components.Count);
        Assert.IsNull(_viewModel.SelectedComponent);
    }

    [TestMethod]
    public void TestSelectedComponentSetter_UpdatesCommandCanExecute()
    {
        // Arrange
        _viewModel.SelectedComponent = null;
        var initialCanExecute = _viewModel.RemoveComponent.CanExecute();

        // Act
        _viewModel.SelectedComponent = _testComponent1;
        var finalCanExecute = _viewModel.RemoveComponent.CanExecute();

        // Assert
        Assert.IsFalse(initialCanExecute);
        Assert.IsTrue(finalCanExecute);
    }

    [TestMethod]
    public void TestRemoveSpecificComponentExecute_WithInvalidParameter_DoesNothing()
    {
        // Arrange
        var initialCount = _viewModel.MyComponents.Count;
        var initialFarmCount = _testFarm.Components.Count;

        // Act
        _viewModel.RemoveSpecificComponentCommand.Execute("invalid parameter");

        // Assert
        Assert.AreEqual(initialCount, _viewModel.MyComponents.Count);
        Assert.AreEqual(initialFarmCount, _testFarm.Components.Count);
    }

    #endregion
}
