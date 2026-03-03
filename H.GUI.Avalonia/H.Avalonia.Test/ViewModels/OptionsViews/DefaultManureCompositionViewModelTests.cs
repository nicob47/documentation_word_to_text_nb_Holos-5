using H.Avalonia.ViewModels.OptionsViews;
using H.Core;
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
    public class DefaultManureCompositionViewModelTests
    {
        private DefaultManureCompositionViewModel _viewModel = null!;
        private Mock<IStorageService> _mockStorageService = null!;
        private IStorageService _storageServiceMock = null!;
        private Mock<IRegionManager> _mockRegionManager = null!;
        private IRegionManager _regionManagerMock = null!;
        private Mock<IEventAggregator> _mockEventAggregator = null!;
        private IEventAggregator _eventAggregatorMock = null!;
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
            _mockStorage = new Mock<IStorage>();
            _storageMock = _mockStorage.Object;
            _applicationData = new ApplicationData();
            _mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void TestConstructorInitializingDTOs()
        {
            var testFarm = new Farm();
            var testDataClassInstance = new DefaultManureCompositionData();
            testDataClassInstance.MoistureContent = 75.0;
            testDataClassInstance.NitrogenFraction = 0.5;
            testDataClassInstance.PhosphorusFraction = 0.3;
            testDataClassInstance.CarbonFraction = 6.0;
            testDataClassInstance.CarbonToNitrogenRatio = 10.0;
            testFarm.DefaultManureCompositionData.Add(testDataClassInstance);
            _mockStorage.Setup(x => x.ApplicationData).Returns(_applicationData);
            _mockStorageService.Setup(x => x.Storage).Returns(_storageMock);
            _mockStorageService.Setup(x => x.GetActiveFarm()).Returns(testFarm);
            
            _viewModel = new DefaultManureCompositionViewModel(_regionManagerMock, _eventAggregatorMock, _storageServiceMock, _mockLogger.Object);

            Assert.AreEqual(1, _viewModel.DefaultManureDTOs.Count);
            Assert.AreEqual(testDataClassInstance.MoistureContent, _viewModel.DefaultManureDTOs[0].MoistureContent);
            Assert.AreEqual(testDataClassInstance.NitrogenFraction, _viewModel.DefaultManureDTOs[0].NitrogenFraction);
            Assert.AreEqual(testDataClassInstance.PhosphorusFraction, _viewModel.DefaultManureDTOs[0].PhosphorusFraction);
            Assert.AreEqual(testDataClassInstance.CarbonFraction, _viewModel.DefaultManureDTOs[0].CarbonFraction);
            Assert.AreEqual(testDataClassInstance.CarbonToNitrogenRatio, _viewModel.DefaultManureDTOs[0].CarbonToNitrogenRatio);
        }

        // Units strings should not change between metric or imperial, testing to ensure that is this is the case 

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

            _viewModel = new DefaultManureCompositionViewModel(_regionManagerMock, _eventAggregatorMock, _storageServiceMock, _mockLogger.Object);
            _viewModel.OnNavigatedTo(null); // implicitly calling private method SetStrings()

            Assert.AreEqual("Total nitrogen (% wet weight)", _viewModel.NitrogenFractionHeader);
            Assert.AreEqual("Total phosphorus (% wet weight)", _viewModel.PhosphorusFractionHeader);
            Assert.AreEqual("Total carbon (% wet weight)", _viewModel.CarbonFractionHeader);
            Assert.AreEqual("Moisture content (%)", _viewModel.MoistureContentHeader);
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

            _viewModel = new DefaultManureCompositionViewModel(_regionManagerMock, _eventAggregatorMock, _storageServiceMock, _mockLogger.Object);
            _viewModel.OnNavigatedTo(null); // implicitly calling private method SetStrings()

            Assert.AreEqual("Total nitrogen (% wet weight)", _viewModel.NitrogenFractionHeader);
            Assert.AreEqual("Total phosphorus (% wet weight)", _viewModel.PhosphorusFractionHeader);
            Assert.AreEqual("Total carbon (% wet weight)", _viewModel.CarbonFractionHeader);
            Assert.AreEqual("Moisture content (%)", _viewModel.MoistureContentHeader);
        }

        [TestMethod]
        public void TestConstructuroThrowsExceptionOnNullConstructorParameter()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new DefaultManureCompositionViewModel(null, null, null, null));
        }
    }
}
