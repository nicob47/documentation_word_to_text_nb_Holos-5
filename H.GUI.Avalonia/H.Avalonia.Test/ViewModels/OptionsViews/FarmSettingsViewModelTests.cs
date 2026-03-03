using System.Collections.ObjectModel;
using H.Avalonia.ViewModels.OptionsViews;
using H.Core;
using H.Core.Enumerations;
using H.Core.Models;
using H.Core.Services.StorageService;
using Moq;
using Prism.Regions;

#nullable disable

namespace H.Avalonia.Test.ViewModels.OptionsViews
{
    [TestClass]
    public class FarmSettingsViewModelTests
    {
        private FarmSettingsViewModel _viewModel = null!;
#pragma warning disable CS0414
        private Mock<IRegionManager> _mockRegionManager = null!;
#pragma warning restore CS0414
#pragma warning disable CS0414
        private IRegionManager _regionManagerMock = null!;
#pragma warning restore CS0414
        private Mock<IStorageService> _mockStorageService = null!;
        private IStorageService _storageServiceMock = null!;
        private Mock<IStorage> _mockStorage = null!;
        private IStorage _storageMock = null!;
        private ApplicationData _applicationData = null!;

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _mockStorageService = new Mock<IStorageService>();
            _storageServiceMock = _mockStorageService.Object;
            _mockStorage = new Mock<IStorage>();
            _storageMock = _mockStorage.Object;

            _applicationData = new ApplicationData();
            _mockStorage.Setup(x => x.ApplicationData).Returns(_applicationData);
            _mockStorageService.Setup(x => x.Storage).Returns(_storageMock);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void TestConstructorInitializingProperties()
        {
            var testMeasurementCollection = new ObservableCollection<MeasurementSystemType>() { MeasurementSystemType.Metric, MeasurementSystemType.Imperial };
            var testFarm = new Farm();
            testFarm.Name = "TestFarm";
            testFarm.Comments = "Test Comments";
            testFarm.Latitude = 12.50;
            testFarm.Longitude = 17.82;
            testFarm.ClimateData.PrecipitationData.GrowingSeasonPrecipitation = 50.66;
            testFarm.ClimateData.EvapotranspirationData.GrowingSeasonEvapotranspiration = 125.27;
            testFarm.MeasurementSystemType = MeasurementSystemType.Metric;
            _mockStorageService.Setup(x => x.GetActiveFarm()).Returns(testFarm);

            _viewModel = new FarmSettingsViewModel(_storageServiceMock); 

            Assert.AreEqual(testFarm.Name, _viewModel.Data.FarmName);
            Assert.AreEqual(testFarm.Comments, _viewModel.Data.FarmComments);
            Assert.AreEqual($"{testFarm.Latitude}, {testFarm.Longitude}", _viewModel.Data.Coordinates);
            Assert.AreEqual(testFarm.ClimateData.PrecipitationData.GrowingSeasonPrecipitation, _viewModel.Data.GrowingSeasonPrecipitation);
            Assert.AreEqual(testFarm.ClimateData.EvapotranspirationData.GrowingSeasonEvapotranspiration, _viewModel.Data.GrowingSeasonEvapotranspiration);
            Assert.AreEqual(testFarm.MeasurementSystemType, _viewModel.SelectedMeasurementSystem);
            Assert.AreEqual(testMeasurementCollection[0], _viewModel.MeasurementSystemTypes[0]);
            Assert.AreEqual(testMeasurementCollection[1], _viewModel.MeasurementSystemTypes[1]);
        }

        [TestMethod]
        public void TestSettingImperial()
        {
            var testFarm = new Farm();
            testFarm.MeasurementSystemType = MeasurementSystemType.Metric;
            testFarm.MeasurementSystemSelected = true;
            _mockStorageService.Setup(x => x.GetActiveFarm()).Returns(testFarm);

            _viewModel = new FarmSettingsViewModel(_storageServiceMock);
            _viewModel.SelectedMeasurementSystem = MeasurementSystemType.Imperial;

            Assert.AreEqual(testFarm.MeasurementSystemType, MeasurementSystemType.Imperial);
            Assert.IsTrue(testFarm.MeasurementSystemSelected);
        }

        [TestMethod]
        public void TestSettingMetric() 
        {
            var testFarm = new Farm();
            testFarm.MeasurementSystemType = MeasurementSystemType.Imperial;
            testFarm.MeasurementSystemSelected = true;
            _mockStorageService.Setup(x => x.GetActiveFarm()).Returns(testFarm);

            _viewModel = new FarmSettingsViewModel(_storageServiceMock);
            _viewModel.SelectedMeasurementSystem = MeasurementSystemType.Metric;

            Assert.AreEqual(testFarm.MeasurementSystemType, MeasurementSystemType.Metric);
            Assert.IsTrue(testFarm.MeasurementSystemSelected);
        }

        [TestMethod]
        public void TestConstructuroThrowsExceptionOnNullConstructorParameter()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new FarmSettingsViewModel(null));
        }
    }
}
