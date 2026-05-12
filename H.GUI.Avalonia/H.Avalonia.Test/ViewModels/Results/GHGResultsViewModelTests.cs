using H.Avalonia.ViewModels.Results;
using H.Core.Models;
using H.Core.Models.Results;
using H.Core.Services.Analysis;
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
        private Mock<IFarmAnalysisService> _mockAnalysisService = null!;

        #endregion

        #region Test Setup

        [TestInitialize]
        public void TestInitialize()
        {
            _mockLogger = new Mock<ILogger>();
            _mockStorageService = new Mock<IStorageService>();
            _mockAnalysisService = new Mock<IFarmAnalysisService>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _viewModel?.Dispose();
        }

        #endregion

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithValidParameters_DoesNotThrow()
        {
            _viewModel = new GHGResultsViewModel(
                _mockLogger.Object,
                _mockStorageService.Object,
                _mockAnalysisService.Object);

            Assert.IsNotNull(_viewModel);
        }

        [TestMethod]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            var exception = Assert.ThrowsExactly<ArgumentNullException>(
                () => new GHGResultsViewModel(null!, _mockStorageService.Object, _mockAnalysisService.Object));

            Assert.AreEqual("logger", exception.ParamName);
        }

        [TestMethod]
        public void Constructor_WithNullStorageService_ThrowsArgumentNullException()
        {
            var exception = Assert.ThrowsExactly<ArgumentNullException>(
                () => new GHGResultsViewModel(_mockLogger.Object, null!, _mockAnalysisService.Object));

            Assert.AreEqual("storageService", exception.ParamName);
        }

        [TestMethod]
        public void Constructor_WithNullAnalysisService_ThrowsArgumentNullException()
        {
            var exception = Assert.ThrowsExactly<ArgumentNullException>(
                () => new GHGResultsViewModel(_mockLogger.Object, _mockStorageService.Object, null!));

            Assert.AreEqual("farmAnalysisService", exception.ParamName);
        }

        [TestMethod]
        public void Constructor_InitializesYearResultsAsEmptyCollection()
        {
            _viewModel = new GHGResultsViewModel(
                _mockLogger.Object,
                _mockStorageService.Object,
                _mockAnalysisService.Object);

            Assert.IsNotNull(_viewModel.YearResults);
            Assert.AreEqual(0, _viewModel.YearResults.Count);
            Assert.IsFalse(_viewModel.HasResults);
        }

        #endregion

        #region RecalculateCommand Tests

        [TestMethod]
        public void RecalculateCommand_WithNoActiveFarm_ClearsResultsAndDoesNotCallService()
        {
            // No active farm → storage service returns null; analysis should be skipped.
            _mockStorageService.Setup(s => s.GetActiveFarm()).Returns((Farm)null!);

            _viewModel = new GHGResultsViewModel(
                _mockLogger.Object,
                _mockStorageService.Object,
                _mockAnalysisService.Object);

            _viewModel.RecalculateCommand.Execute();

            _mockAnalysisService.Verify(s => s.RunAnalysis(It.IsAny<Farm>()), Times.Never);
            Assert.IsFalse(_viewModel.HasResults);
            Assert.IsNull(_viewModel.LastErrorMessage);
        }

        [TestMethod]
        public void RecalculateCommand_WithActiveFarm_PopulatesYearResultsFromService()
        {
            var farm = new Farm { Name = "Test Farm" };
            _mockStorageService.Setup(s => s.GetActiveFarm()).Returns(farm);

            var analysisResults = new FarmAnalysisResults
            {
                FarmName = "Test Farm",
                CarbonModellingStrategy = "ICBM",
                YearResults = new[]
                {
                    new FieldAnalysisYearResult { Year = 2020, FieldName = "F1", SoilCarbon = 50.0 },
                    new FieldAnalysisYearResult { Year = 2021, FieldName = "F1", SoilCarbon = 51.2 },
                },
            };
            _mockAnalysisService.Setup(s => s.RunAnalysis(farm)).Returns(analysisResults);

            _viewModel = new GHGResultsViewModel(
                _mockLogger.Object,
                _mockStorageService.Object,
                _mockAnalysisService.Object);

            _viewModel.RecalculateCommand.Execute();

            Assert.AreEqual("Test Farm", _viewModel.FarmName);
            Assert.AreEqual("ICBM", _viewModel.CarbonModellingStrategy);
            Assert.AreEqual(2, _viewModel.YearResults.Count);
            Assert.IsTrue(_viewModel.HasResults);
            Assert.IsNull(_viewModel.LastErrorMessage);
        }

        [TestMethod]
        public void RecalculateCommand_WhenServiceThrows_CapturesErrorMessageAndClearsResults()
        {
            var farm = new Farm { Name = "Test Farm" };
            _mockStorageService.Setup(s => s.GetActiveFarm()).Returns(farm);
            _mockAnalysisService.Setup(s => s.RunAnalysis(farm))
                .Throws(new InvalidOperationException("calculator misconfigured"));

            _viewModel = new GHGResultsViewModel(
                _mockLogger.Object,
                _mockStorageService.Object,
                _mockAnalysisService.Object);

            _viewModel.RecalculateCommand.Execute();

            Assert.IsFalse(_viewModel.HasResults);
            Assert.AreEqual("calculator misconfigured", _viewModel.LastErrorMessage);
        }

        #endregion
    }
}
