using H.Avalonia.ViewModels.OptionsViews;
using H.Core;
using H.Core.Calculators.UnitsOfMeasurement;
using H.Core.Enumerations;
using H.Core.Models;
using H.Core.Providers.Animals;
using H.Core.Services.StorageService;
using Microsoft.Extensions.Logging;
using Moq;
using Prism.Events;
using Prism.Regions;

#nullable disable

namespace H.Avalonia.Test.ViewModels.OptionsViews
{
    [TestClass]
    public class DefaultBeddingCompositionViewModelTests
    {
        private DefaultBeddingCompositionViewModel _viewModel = null!;
        private Mock<IStorageService> _mockStorageService = null!;
        private IStorageService _storageServiceMock = null!;
        private Mock<IRegionManager> _mockRegionManager = null!;
        private IRegionManager _regionManagerMock = null!;
        private Mock<IEventAggregator> _mockEventAggregator = null!;
        private IEventAggregator _eventAggregatorMock = null!;
        private Mock<IUnitsOfMeasurementCalculator> _mockUnitsCalculator = null!;
        private IUnitsOfMeasurementCalculator _unitsCalculatorMock = null!;
        private Mock<IStorage> _mockStorage = null!;
        private IStorage _storageMock = null!;
        private ApplicationData _applicationData = null!;
        private Mock<ILogger> _mockLogger = null!;

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
            _mockRegionManager = new Mock<IRegionManager>();
            _regionManagerMock = _mockRegionManager.Object;
            _mockEventAggregator = new Mock<IEventAggregator>();
            _eventAggregatorMock = _mockEventAggregator.Object;
            _mockStorageService = new Mock<IStorageService>();
            _storageServiceMock = _mockStorageService.Object;
            _mockUnitsCalculator = new Mock<IUnitsOfMeasurementCalculator>();
            _unitsCalculatorMock = _mockUnitsCalculator.Object;
            _mockStorage = new Mock<IStorage>();
            _storageMock = _mockStorage.Object;
            _applicationData = new ApplicationData();
            _mockLogger = new Mock<ILogger>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void TestConstructorInitializingDTOs()
        {
            var testFarm = new Farm();
            var testDataClassInstance = new Table_30_Default_Bedding_Material_Composition_Data();
            testDataClassInstance.TotalNitrogenKilogramsDryMatter = 0.005;
            testDataClassInstance.TotalPhosphorusKilogramsDryMatter = 0.001;
            testDataClassInstance.TotalCarbonKilogramsDryMatter = 0.3;
            testDataClassInstance.CarbonToNitrogenRatio = 50.0;
            testFarm.DefaultsCompositionOfBeddingMaterials.Add(testDataClassInstance);

            _mockStorage.Setup(x => x.ApplicationData).Returns(_applicationData);
            _mockStorageService.Setup(x => x.Storage).Returns(_storageMock);
            _mockStorageService.Setup(x => x.GetActiveFarm()).Returns(testFarm);
            _mockUnitsCalculator.Setup(x => x.IsMetric).Returns(true);

            _viewModel = new DefaultBeddingCompositionViewModel(_regionManagerMock, _eventAggregatorMock, _storageServiceMock, _unitsCalculatorMock, _mockLogger.Object);

            Assert.AreEqual(1, _viewModel.BeddingCompositionDTOs.Count);
            Assert.AreEqual(testDataClassInstance.TotalNitrogenKilogramsDryMatter, _viewModel.BeddingCompositionDTOs[0].TotalNitrogenKilogramsDryMatter);
            Assert.AreEqual(testDataClassInstance.TotalPhosphorusKilogramsDryMatter, _viewModel.BeddingCompositionDTOs[0].TotalPhosphorusKilogramsDryMatter);
            Assert.AreEqual(testDataClassInstance.TotalCarbonKilogramsDryMatter, _viewModel.BeddingCompositionDTOs[0].TotalCarbonKilogramsDryMatter);
            Assert.AreEqual(testDataClassInstance.CarbonToNitrogenRatio, _viewModel.BeddingCompositionDTOs[0].CarbonToNitrogenRatio);
        }

        [TestMethod]
        public void TestSetStringsMetric()
        {
            var testFarm = new Farm();
            testFarm.MeasurementSystemType = MeasurementSystemType.Metric;
            var displayUnitsInstance = new DisplayUnitStrings();
            displayUnitsInstance.SetStrings(testFarm.MeasurementSystemType);
            _applicationData.DisplayUnitStrings = displayUnitsInstance;

            _mockStorage.Setup(x => x.ApplicationData).Returns(_applicationData);
            _mockStorageService.Setup(x => x.Storage).Returns(_storageMock);
            _mockStorageService.Setup(x => x.GetActiveFarm()).Returns(testFarm);

            _viewModel = new DefaultBeddingCompositionViewModel(_regionManagerMock, _eventAggregatorMock, _storageServiceMock, _unitsCalculatorMock, _mockLogger.Object);

            _viewModel.OnNavigatedTo(null); // implicitly calling private method SetStrings()

            Assert.AreEqual("Total nitrogen (kg N (kg DM)^1)", _viewModel.NitrogenConcentrationHeader);
            Assert.AreEqual("Total phosphorus (kg P (kg DM)^1)", _viewModel.PhosphorusConcentrationHeader);
            Assert.AreEqual("Total carbon (kg C (kg DM)^1)", _viewModel.CarbonConcentrationHeader);
        }

        [TestMethod]
        public void TestSetStringsImperial()
        {
            var testFarm = new Farm();
            testFarm.MeasurementSystemType = MeasurementSystemType.Imperial;
            var displayUnitsInstance = new DisplayUnitStrings();
            displayUnitsInstance.SetStrings(testFarm.MeasurementSystemType);
            _applicationData.DisplayUnitStrings = displayUnitsInstance;

            _mockStorage.Setup(x => x.ApplicationData).Returns(_applicationData);
            _mockStorageService.Setup(x => x.Storage).Returns(_storageMock);
            _mockStorageService.Setup(x => x.GetActiveFarm()).Returns(testFarm);

            _viewModel = new DefaultBeddingCompositionViewModel(_regionManagerMock, _eventAggregatorMock, _storageServiceMock, _unitsCalculatorMock, _mockLogger.Object);

            _viewModel.OnNavigatedTo(null); // implicitly calling private method SetStrings()

            Assert.AreEqual("Total nitrogen (lb N (lb DM)^-1)", _viewModel.NitrogenConcentrationHeader);
            Assert.AreEqual("Total phosphorus (lb P (lb DM)^-1)", _viewModel.PhosphorusConcentrationHeader);
            Assert.AreEqual("Total carbon (lb C (lb DM)^-1)", _viewModel.CarbonConcentrationHeader);
        }

        [TestMethod]
        public void TestConstructorThrowsExceptionOnNullConstructorParameter()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new DefaultBeddingCompositionViewModel(null, null, null, null, null));
        }
    }
}
