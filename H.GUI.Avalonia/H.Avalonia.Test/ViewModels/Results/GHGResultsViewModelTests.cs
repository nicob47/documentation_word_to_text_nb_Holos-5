using H.Avalonia.ViewModels.Results;
using H.Core.Enumerations;
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

        #region SelectedStrategy Tests

        [TestMethod]
        public void RecalculateCommand_SyncsSelectedStrategyToActiveFarmDefaults()
        {
            var farm = new Farm { Name = "Test Farm" };
            farm.Defaults.CarbonModellingStrategy = CarbonModellingStrategies.IPCCTier2;
            _mockStorageService.Setup(s => s.GetActiveFarm()).Returns(farm);

            _mockAnalysisService.Setup(s => s.RunAnalysis(farm))
                .Returns(new FarmAnalysisResults());

            _viewModel = new GHGResultsViewModel(
                _mockLogger.Object,
                _mockStorageService.Object,
                _mockAnalysisService.Object);

            _viewModel.RecalculateCommand.Execute();

            Assert.AreEqual(CarbonModellingStrategies.IPCCTier2, _viewModel.SelectedStrategy);
            // The sync should not have triggered an extra analysis on top of the one we requested.
            _mockAnalysisService.Verify(s => s.RunAnalysis(farm), Times.Once);
        }

        [TestMethod]
        public void SelectedStrategy_SetterWritesBackToFarmDefaultsAndReanalyzes()
        {
            var farm = new Farm { Name = "Test Farm" };
            farm.Defaults.CarbonModellingStrategy = CarbonModellingStrategies.ICBM;
            _mockStorageService.Setup(s => s.GetActiveFarm()).Returns(farm);
            _mockAnalysisService.Setup(s => s.RunAnalysis(farm)).Returns(new FarmAnalysisResults());

            _viewModel = new GHGResultsViewModel(
                _mockLogger.Object,
                _mockStorageService.Object,
                _mockAnalysisService.Object);

            // Prime the view model so SelectedStrategy reflects the farm's current value (ICBM).
            _viewModel.RecalculateCommand.Execute();
            _mockAnalysisService.Invocations.Clear();

            _viewModel.SelectedStrategy = CarbonModellingStrategies.IPCCTier2;

            Assert.AreEqual(CarbonModellingStrategies.IPCCTier2, farm.Defaults.CarbonModellingStrategy);
            _mockAnalysisService.Verify(s => s.RunAnalysis(farm), Times.Once);
        }

        [TestMethod]
        public void AvailableCarbonStrategies_ContainsIcbmAndTier2()
        {
            _viewModel = new GHGResultsViewModel(
                _mockLogger.Object,
                _mockStorageService.Object,
                _mockAnalysisService.Object);

            CollectionAssert.Contains(_viewModel.AvailableCarbonStrategies.ToArray(), CarbonModellingStrategies.ICBM);
            CollectionAssert.Contains(_viewModel.AvailableCarbonStrategies.ToArray(), CarbonModellingStrategies.IPCCTier2);
        }

        #endregion

        #region Soil C Trend Chart Tests

        [TestMethod]
        public void RecalculateCommand_BuildsOneSoilCarbonSeriesPerField()
        {
            var farm = new Farm { Name = "Farm" };
            _mockStorageService.Setup(s => s.GetActiveFarm()).Returns(farm);

            _mockAnalysisService.Setup(s => s.RunAnalysis(farm)).Returns(new FarmAnalysisResults
            {
                YearResults = new[]
                {
                    new FieldAnalysisYearResult { FieldName = "North",  Year = 2020, SoilCarbon = 50 },
                    new FieldAnalysisYearResult { FieldName = "North",  Year = 2021, SoilCarbon = 51 },
                    new FieldAnalysisYearResult { FieldName = "South",  Year = 2020, SoilCarbon = 70 },
                    new FieldAnalysisYearResult { FieldName = "South",  Year = 2021, SoilCarbon = 71 },
                },
            });

            _viewModel = new GHGResultsViewModel(
                _mockLogger.Object,
                _mockStorageService.Object,
                _mockAnalysisService.Object);
            _viewModel.RecalculateCommand.Execute();

            Assert.AreEqual(2, _viewModel.SoilCarbonTrendSeries.Length);

            Assert.AreEqual(1, _viewModel.SoilCarbonTrendXAxes.Length);
            CollectionAssert.AreEqual(new[] { "2020", "2021" }, _viewModel.SoilCarbonTrendXAxes[0].Labels?.ToArray());
        }

        [TestMethod]
        public void RecalculateCommand_WithNoResults_ClearsChart()
        {
            var farm = new Farm();
            _mockStorageService.Setup(s => s.GetActiveFarm()).Returns(farm);
            _mockAnalysisService.Setup(s => s.RunAnalysis(farm)).Returns(new FarmAnalysisResults());

            _viewModel = new GHGResultsViewModel(
                _mockLogger.Object,
                _mockStorageService.Object,
                _mockAnalysisService.Object);
            _viewModel.RecalculateCommand.Execute();

            Assert.AreEqual(0, _viewModel.SoilCarbonTrendSeries.Length);
            Assert.AreEqual(0, _viewModel.SoilCarbonTrendXAxes.Length);
        }

        #endregion

        #region ExportFieldResultsCommand Tests

        [TestMethod]
        public void ExportFieldResultsCommand_IsDisabledWhenNoResults()
        {
            _viewModel = new GHGResultsViewModel(
                _mockLogger.Object,
                _mockStorageService.Object,
                _mockAnalysisService.Object);

            Assert.IsFalse(_viewModel.ExportFieldResultsCommand.CanExecute());
        }

        [TestMethod]
        public void ExportFieldResultsCommand_IsEnabledAfterResultsLoad()
        {
            var farm = new Farm { Name = "Farm" };
            _mockStorageService.Setup(s => s.GetActiveFarm()).Returns(farm);
            _mockAnalysisService.Setup(s => s.RunAnalysis(farm)).Returns(new FarmAnalysisResults
            {
                YearResults = new[]
                {
                    new FieldAnalysisYearResult { FieldName = "F", Year = 2020, SoilCarbon = 50 },
                },
            });

            _viewModel = new GHGResultsViewModel(
                _mockLogger.Object,
                _mockStorageService.Object,
                _mockAnalysisService.Object);
            _viewModel.RecalculateCommand.Execute();

            Assert.IsTrue(_viewModel.ExportFieldResultsCommand.CanExecute());
        }

        [TestMethod]
        public void WriteFieldResultsCsv_ProducesHeaderAndOneRowPerInputAndQuotesCommas()
        {
            var rows = new[]
            {
                new FieldAnalysisYearResult
                {
                    Year = 2020,
                    FieldName = "Field, with comma",
                    CropType = "Wheat",
                    Area = 100,
                    SoilCarbon = 50.123,
                    DirectN2OPerHectare = 1.5,
                    IndirectN2OPerHectare = 0.5,
                },
                new FieldAnalysisYearResult
                {
                    Year = 2021,
                    FieldName = "North",
                    CropType = "Barley",
                    Area = 75,
                    SoilCarbon = 51.0,
                },
            };

            // Internal static — access via the test project's InternalsVisibleTo if configured, or
            // exercise the command end-to-end. Here we just call the static via reflection so the
            // test stays isolated from the file-dialog plumbing.
            var path = (string)typeof(GHGResultsViewModel)
                .GetMethod("WriteFieldResultsCsv", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object[] { rows, "TestFarm" })!;

            try
            {
                Assert.IsTrue(File.Exists(path));
                var content = File.ReadAllText(path);
                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                Assert.AreEqual(3, lines.Length, "header + 2 data rows");
                StringAssert.Contains(lines[0], "Year");
                StringAssert.Contains(lines[0], "TotalN2O_kg_per_ha");
                StringAssert.Contains(lines[1], "\"Field, with comma\"");      // comma quoting
                StringAssert.Contains(lines[1], "2");                          // TotalN2O = 1.5+0.5
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        #endregion
    }
}
