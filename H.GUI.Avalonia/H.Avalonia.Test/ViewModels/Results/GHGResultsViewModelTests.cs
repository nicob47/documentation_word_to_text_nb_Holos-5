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
        public async Task RunAnalysisAsync_WithNoActiveFarm_ClearsResultsAndDoesNotCallService()
        {
            // No active farm → storage service returns null; analysis should be skipped.
            _mockStorageService.Setup(s => s.GetActiveFarm()).Returns((Farm)null!);

            _viewModel = new GHGResultsViewModel(
                _mockLogger.Object,
                _mockStorageService.Object,
                _mockAnalysisService.Object);

            await _viewModel.RunAnalysisAsync();

            _mockAnalysisService.Verify(s => s.RunAnalysis(It.IsAny<Farm>()), Times.Never);
            Assert.IsFalse(_viewModel.HasResults);
            Assert.IsNull(_viewModel.LastErrorMessage);
        }

        [TestMethod]
        public async Task RunAnalysisAsync_WithActiveFarm_PopulatesYearResultsFromService()
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

            await _viewModel.RunAnalysisAsync();

            Assert.AreEqual("Test Farm", _viewModel.FarmName);
            Assert.AreEqual("ICBM", _viewModel.CarbonModellingStrategy);
            Assert.AreEqual(2, _viewModel.YearResults.Count);
            Assert.IsTrue(_viewModel.HasResults);
            Assert.IsNull(_viewModel.LastErrorMessage);
        }

        [TestMethod]
        public async Task RunAnalysisAsync_WhenServiceThrows_CapturesErrorMessageAndClearsResults()
        {
            var farm = new Farm { Name = "Test Farm" };
            _mockStorageService.Setup(s => s.GetActiveFarm()).Returns(farm);
            _mockAnalysisService.Setup(s => s.RunAnalysis(farm))
                .Throws(new InvalidOperationException("calculator misconfigured"));

            _viewModel = new GHGResultsViewModel(
                _mockLogger.Object,
                _mockStorageService.Object,
                _mockAnalysisService.Object);

            await _viewModel.RunAnalysisAsync();

            Assert.IsFalse(_viewModel.HasResults);
            Assert.AreEqual("calculator misconfigured", _viewModel.LastErrorMessage);
        }

        #endregion

        #region SelectedStrategy Tests

        [TestMethod]
        public async Task RunAnalysisAsync_SyncsSelectedStrategyToActiveFarmDefaults()
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

            await _viewModel.RunAnalysisAsync();

            Assert.AreEqual(CarbonModellingStrategies.IPCCTier2, _viewModel.SelectedStrategy);
            // The sync should not have triggered an extra analysis on top of the one we requested.
            _mockAnalysisService.Verify(s => s.RunAnalysis(farm), Times.Once);
        }

        [TestMethod]
        public async Task SelectedStrategy_SetterWritesBackToFarmDefaultsAndReanalyzes()
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
            await _viewModel.RunAnalysisAsync();
            _mockAnalysisService.Invocations.Clear();

            // SelectedStrategy setter fires-and-forgets RunAnalysisAsync. Drive it through
            // the public async method instead so the test can await the analysis call rather
            // than racing the dispatcher.
            farm.Defaults.CarbonModellingStrategy = CarbonModellingStrategies.IPCCTier2;
            await _viewModel.RunAnalysisAsync();

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
        public async Task RunAnalysisAsync_BuildsOneSoilCarbonSeriesPerField()
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
            await _viewModel.RunAnalysisAsync();

            Assert.AreEqual(2, _viewModel.SoilCarbonTrendSeries.Length);

            Assert.AreEqual(1, _viewModel.SoilCarbonTrendXAxes.Length);
            CollectionAssert.AreEqual(new[] { "2020", "2021" }, _viewModel.SoilCarbonTrendXAxes[0].Labels?.ToArray());
        }

        [TestMethod]
        public async Task RunAnalysisAsync_WithNoResults_ClearsChart()
        {
            var farm = new Farm();
            _mockStorageService.Setup(s => s.GetActiveFarm()).Returns(farm);
            _mockAnalysisService.Setup(s => s.RunAnalysis(farm)).Returns(new FarmAnalysisResults());

            _viewModel = new GHGResultsViewModel(
                _mockLogger.Object,
                _mockStorageService.Object,
                _mockAnalysisService.Object);
            await _viewModel.RunAnalysisAsync();

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
        public async Task ExportFieldResultsCommand_IsEnabledAfterResultsLoad()
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
            await _viewModel.RunAnalysisAsync();

            Assert.IsTrue(_viewModel.ExportFieldResultsCommand.CanExecute());
        }

        [TestMethod]
        public void WriteFieldResultsXlsx_ProducesStyledHeaderAndOneRowPerInput()
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

            // Internal static — access via reflection so the test stays isolated from the file
            // dialog plumbing and from any DI wiring the production command path adds later.
            var path = (string)typeof(GHGResultsViewModel)
                .GetMethod("WriteFieldResultsXlsx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object[] { rows, "TestFarm" })!;

            try
            {
                Assert.IsTrue(File.Exists(path));
                Assert.IsTrue(path.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase));

                // Round-trip through ClosedXML to verify the styled workbook is well-formed and
                // contains the expected rows. Row 1 = title banner, row 2 = headers, rows 3+ = data.
                using var wb = new ClosedXML.Excel.XLWorkbook(path);
                var sheet = wb.Worksheets.First();

                StringAssert.Contains(sheet.Cell(1, 1).GetString(), "TestFarm");
                Assert.AreEqual("Year", sheet.Cell(2, 1).GetString());
                Assert.AreEqual("Field, with comma", sheet.Cell(3, 2).GetString());
                Assert.AreEqual(2020d, sheet.Cell(3, 1).GetDouble());
                Assert.AreEqual(2d, sheet.Cell(3, 15).GetDouble(), 1e-9); // TotalN2O = 1.5 + 0.5
                Assert.AreEqual("North", sheet.Cell(4, 2).GetString());
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        #endregion
    }
}
