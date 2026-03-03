using H.Avalonia.ViewModels.OptionsViews;
using H.Core;
using H.Core.Models;
using H.Core.Providers.Temperature;
using H.Core.Services.StorageService;
using Moq;

#nullable disable

namespace H.Avalonia.Test.ViewModels.OptionsViews
{
    [TestClass]
    public class TemperatureSettingsViewModelTests
    {
        private Farm _testFarm = null!;
        private TemperatureData _temperatureData = null!;
        private TemperatureSettingsViewModel _viewModel = null!;
        private Mock<IStorageService> _mockStorageService = null!;
        private IStorageService _storageServiceMock = null!;
        private Mock<IStorage> _mockStorage = null!;
        private IStorage _storageMock = null!;
        private ApplicationData _applicationData = null!;

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
            _mockStorageService = new Mock<IStorageService>();
            _storageServiceMock = _mockStorageService.Object;
            _mockStorage = new Mock<IStorage>();
            _storageMock = _mockStorage.Object;

            _applicationData = new ApplicationData();
            _mockStorage.Setup(x => x.ApplicationData).Returns(_applicationData);
            _mockStorageService.Setup(x => x.Storage).Returns(_storageMock);

            _testFarm = new Farm();
            _temperatureData = new TemperatureData
            {
                January = 1.0,
                February = -2.0,
                March = 3.5,
                April = -9.0,
                May = 4.7,
                June = -0.8,
                July = 5.4,
                August = -6.1,
                September = 7.3,
                October = 13.6,
                November = 8.6,
                December = -4.2
            };
            _testFarm.ClimateData.TemperatureData = _temperatureData;
            _mockStorageService.Setup(x => x.GetActiveFarm()).Returns(_testFarm);
        }

        [TestMethod]
        public void TestInitializationLogic()
        {
            _viewModel = new TemperatureSettingsViewModel(_storageServiceMock);

            Assert.AreEqual(1.0, _viewModel.Data.January);
            Assert.AreEqual(-2.0, _viewModel.Data.February);
            Assert.AreEqual(3.5, _viewModel.Data.March);
            Assert.AreEqual(-9.0, _viewModel.Data.April);
            Assert.AreEqual(4.7, _viewModel.Data.May);
            Assert.AreEqual(-0.8, _viewModel.Data.June);
            Assert.AreEqual(5.4, _viewModel.Data.July);
            Assert.AreEqual(-6.1, _viewModel.Data.August);
            Assert.AreEqual(7.3, _viewModel.Data.September);
            Assert.AreEqual(13.6, _viewModel.Data.October);
            Assert.AreEqual(8.6, _viewModel.Data.November);
            Assert.AreEqual(-4.2, _viewModel.Data.December);
        }

        [TestMethod]
        public void TestConstructuroThrowsExceptionOnNullConstructorParameter()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new TemperatureSettingsViewModel(null));
        }
    }
}
