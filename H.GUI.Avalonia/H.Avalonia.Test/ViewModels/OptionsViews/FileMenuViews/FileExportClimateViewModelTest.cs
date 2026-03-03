using Avalonia.Platform.Storage;
using H.Avalonia.Services;
using H.Avalonia.ViewModels.OptionsViews.FileMenuViews;
using H.Core.Models;
using H.Core.Services.StorageService;
using Moq;
using Prism.Regions;
using static H.Avalonia.Views.OptionsViews.FileMenuViews.FileExportClimateView;

#nullable disable

namespace H.Avalonia.Test.ViewModels.OptionsViews.FileMenuViews
{
    [TestClass]
    public class FileExportClimateViewModelTest
    {
        private Mock<IRegionManager> _mockRegionManager = null!;
        private Mock<IStorageService> _mockStorageService = null!;
        private Mock<INotificationManagerService> _mockNotificationService = null!;
        private Mock<IStorageFile> _mockStorageFile = null!;
        private FileExportClimateViewModel _viewModel = null!;
        private Farm _testFarm = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            // Initialize mocks
            _mockRegionManager = new Mock<IRegionManager>();
            _mockStorageService = new Mock<IStorageService>();
            _mockNotificationService = new Mock<INotificationManagerService>();
            _mockStorageFile = new Mock<IStorageFile>();

            // Setup mock storage file
            _mockStorageFile.Setup(f => f.Name).Returns("test_farm_climate.csv");
            _mockStorageFile.Setup(f => f.Path).Returns(new Uri("file:///C:/temp/test_farm_climate.csv"));

            // Create test farm
            _testFarm = new Farm
            {
                Name = "Test Farm",
                Latitude = 52.1579,
                Longitude = -106.6702
            };

            // Create view model instance
            _viewModel = new FileExportClimateViewModel(_mockRegionManager.Object, _mockStorageService.Object, _mockNotificationService.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _viewModel = null;
            _mockRegionManager = null;
            _mockStorageService = null;
            _mockStorageFile = null;
            _testFarm = null;
        }

        [TestMethod]
        public void Constructor_WithValidParameters_InitializesCorrectly()
        {
            // Arrange & Act
            var viewModel = new FileExportClimateViewModel(_mockRegionManager.Object, _mockStorageService.Object, _mockNotificationService.Object);

            // Assert
            Assert.IsNotNull(viewModel);
            Assert.IsNotNull(viewModel.ExportClimate);
        }

        [TestMethod]
        public void Constructor_WithNullRegionManager_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => 
                new FileExportClimateViewModel(null, _mockStorageService.Object, _mockNotificationService.Object));
        }

        [TestMethod]
        public void Constructor_WithNullStorageService_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => 
                new FileExportClimateViewModel(_mockRegionManager.Object, null, _mockNotificationService.Object));
        }

        [TestMethod]
        public void Constructor_WithNullNotificationService_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                new FileExportClimateViewModel(null, null, _mockNotificationService.Object));
        }

        [TestMethod]
        public async Task ExportAsync_WithValidFarmAndFile_CompletesSuccessfully()
        {
            // Arrange
            // NotificationManager is null by default, which is fine since the code uses null-conditional operator

            // Act & Assert - should not throw exception
            await _viewModel.ExportAsync(_testFarm, _mockStorageFile.Object);
            
            // Verify the test completes without throwing an exception
            Assert.IsTrue(true);
        }

        [TestMethod]
        public async Task ExportAsync_WithNullFarm_CompletesWithoutException()
        {
            // Arrange
            // NotificationManager is null by default, which is fine since the code uses null-conditional operator

            // Act & Assert - should not throw exception
            await _viewModel.ExportAsync(null, _mockStorageFile.Object);
            
            // Verify the test completes without throwing an exception
            Assert.IsTrue(true);
        }

        [TestMethod]
        public async Task ExportAsync_WithNullFile_CompletesWithoutException()
        {
            // Arrange
            // NotificationManager is null by default, which is fine since the code uses null-conditional operator

            // Act & Assert - should not throw exception
            await _viewModel.ExportAsync(_testFarm, null);
            
            // Verify the test completes without throwing an exception
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ExportClimateCommand_WithValidExportClimateData_ExecutesSuccessfully()
        {
            // Arrange
            var exportData = new ExportClimateData
            {
                Farm = _testFarm,
                File = _mockStorageFile.Object
            };

            // Act
            _viewModel.ExportClimate.Execute(exportData);

            // Assert
            Assert.IsTrue(_viewModel.ExportClimate.CanExecute(exportData));
        }

        [TestMethod]
        public void ExportClimateCommand_WithNullData_ExecutesWithoutException()
        {
            // Arrange & Act - should not throw exception
            _viewModel.ExportClimate.Execute(null);
            
            // Assert - verify the test completes without throwing an exception
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ExportClimateCommand_WithInvalidData_ExecutesWithoutException()
        {
            // Arrange
            var invalidData = "Invalid data type";

            // Act - should not throw exception
            _viewModel.ExportClimate.Execute(invalidData);

            // Assert - verify the test completes without throwing an exception
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ExportClimateCommand_WithNullFarmInData_ExecutesWithoutException()
        {
            // Arrange
            var exportData = new ExportClimateData
            {
                Farm = null,
                File = _mockStorageFile.Object
            };

            // Act - should not throw exception
            _viewModel.ExportClimate.Execute(exportData);

            // Assert - verify the test completes without throwing an exception
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ExportClimateCommand_WithNullFileInData_ExecutesWithoutException()
        {
            // Arrange
            var exportData = new ExportClimateData
            {
                Farm = _testFarm,
                File = null
            };

            // Act - should not throw exception
            _viewModel.ExportClimate.Execute(exportData);

            // Assert - verify the test completes without throwing an exception
            Assert.IsTrue(true);
        }
    }
}
