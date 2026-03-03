using H.Avalonia.ViewModels.Results;
using H.Core.Services.StorageService;
using Microsoft.Extensions.Logging;
using Moq;

#nullable disable

namespace H.Avalonia.Test.ViewModels.Results
{
    [TestClass]
    public class GHGResultsViewModelTests
    {
        #region Fields

        private GHGResultsViewModel _viewModel = null!;
        private Mock<ILogger> _mockLogger = null!;
        private Mock<IStorageService> _mockStorageService = null!;

        #endregion

        #region Test Setup

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
            _mockLogger = new Mock<ILogger>();
            _mockStorageService = new Mock<IStorageService>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _viewModel?.Dispose();
        }

        #endregion

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithValidParameters_SetsLoggerCorrectly()
        {
            // Arrange
            var logger = _mockLogger.Object;
            var storageService = _mockStorageService.Object;

            // Act
            _viewModel = new GHGResultsViewModel(logger, storageService);

            // Assert
            Assert.IsNotNull(_viewModel);
            // Note: _logger is a private field, so we can't directly test it
            // but we can verify the constructor doesn't throw
        }

        [TestMethod]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            ILogger nullLogger = null;
            var storageService = _mockStorageService.Object;

            // Act & Assert
            var exception = Assert.ThrowsExactly<ArgumentNullException>(
                () => new GHGResultsViewModel(nullLogger, storageService));
            
            Assert.AreEqual("logger", exception.ParamName);
        }

        [TestMethod]
        public void Constructor_WithValidLogger_InitializesResultsPropertyToNull()
        {
            // Arrange
            var logger = _mockLogger.Object;
            var storageService = _mockStorageService.Object;

            // Act
            _viewModel = new GHGResultsViewModel(logger, storageService);

            // Assert
            Assert.IsNull(_viewModel.Results);
        }

        #endregion
    }
}
