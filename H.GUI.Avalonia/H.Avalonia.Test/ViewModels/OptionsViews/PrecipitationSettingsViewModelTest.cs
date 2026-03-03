using H.Avalonia.ViewModels.OptionsViews;
using H.Core;
using H.Core.Models;
using H.Core.Providers.Precipitation;
using H.Core.Services.StorageService;
using Moq;

#nullable disable

namespace H.Avalonia.Test.ViewModels.OptionsViews
{
    [TestClass]
    public class PrecipitationSettingsViewModelTests
    {
        private Farm _testFarm = null!;
        private PrecipitationData _precipitationData = null!;
        private PrecipitationSettingsViewModel _viewModel = null!;
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
            _precipitationData = new PrecipitationData
            {
                January = 1.0,
                February = 2.0,
                March = 3.0,
                April = 4.0,
                May = 5.0,
                June = 6.0,
                July = 7.0,
                August = 8.0,
                September = 9.0,
                October = 10.0,
                November = 11.0,
                December = 12.0
            };
            _testFarm.ClimateData.PrecipitationData = _precipitationData;
            _mockStorageService.Setup(x => x.GetActiveFarm()).Returns(_testFarm);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void TestInitializationLogic()
        {
            _viewModel = new PrecipitationSettingsViewModel(_storageServiceMock);

            Assert.AreEqual(1.0, _viewModel.Data.January);
            Assert.AreEqual(2.0, _viewModel.Data.February);
            Assert.AreEqual(3.0, _viewModel.Data.March);
            Assert.AreEqual(4.0, _viewModel.Data.April);
            Assert.AreEqual(5.0, _viewModel.Data.May);
            Assert.AreEqual(6.0, _viewModel.Data.June);
            Assert.AreEqual(7.0, _viewModel.Data.July);
            Assert.AreEqual(8.0, _viewModel.Data.August);
            Assert.AreEqual(9.0, _viewModel.Data.September);
            Assert.AreEqual(10.0, _viewModel.Data.October);
            Assert.AreEqual(11.0, _viewModel.Data.November);
            Assert.AreEqual(12.0, _viewModel.Data.December);
        }

        [TestMethod]
        public void TestConstructuroThrowsExceptionOnNullConstructorParameter()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new PrecipitationSettingsViewModel(null));
        }
    }
}
