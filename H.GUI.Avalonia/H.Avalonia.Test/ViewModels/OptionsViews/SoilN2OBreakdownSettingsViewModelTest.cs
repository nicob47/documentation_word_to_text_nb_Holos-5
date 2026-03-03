using H.Avalonia.Services;
using H.Avalonia.ViewModels.OptionsViews;
using H.Core;
using H.Core.Enumerations;
using H.Core.Models;
using H.Core.Providers.Soil;
using H.Core.Services.StorageService;
using Moq;
using Prism.Events;
using H.Avalonia.Events;

#nullable disable

namespace H.Avalonia.Test.ViewModels.OptionsViews
{
    [TestClass]
    public class SoilN2OBreakdownSettingsViewModelTest
    {
        private SoilN2OBreakdownSettingsViewModel _viewModel = null!;
        private Mock<IStorageService> _mockStorageService = null!;
        private IStorageService _storageServiceMock = null!;
        private Mock<IStorage> _mockStorage = null!;
        private IStorage _storageMock = null!;
        private Mock<IErrorHandlerService> _mockErrorHandlerService = null!;
        private IErrorHandlerService _errorHandlerServiceMock = null!;
        private Mock<IEventAggregator> _mockEventAggregator = null!;
        private IEventAggregator _eventAggregatorMock = null!;

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
            // Storage service setup
            _mockStorageService = new Mock<IStorageService>();
            _storageServiceMock = _mockStorageService.Object;

            _mockStorage = new Mock<IStorage>();
            _storageMock = _mockStorage.Object;

            var globalSettings = new GlobalSettings();
            var applicationData = new ApplicationData();
            applicationData.GlobalSettings = globalSettings;

            _mockStorage.Setup(x => x.ApplicationData).Returns(applicationData);
            _mockStorageService.Setup(x => x.Storage).Returns(_storageMock);

            // Error handler service setup
            _mockErrorHandlerService = new Mock<IErrorHandlerService>();
            _errorHandlerServiceMock = _mockErrorHandlerService.Object;
            _mockErrorHandlerService.Setup(x => x.HandleValidationWarning(It.IsAny<string>(), It.IsAny<string>()));

            // Event aggregator setup
            _mockEventAggregator = new Mock<IEventAggregator>();
            _eventAggregatorMock = _mockEventAggregator.Object;
        }

        [TestMethod]
        public void TestConstructorInitialization()
        {
            _viewModel = new SoilN2OBreakdownSettingsViewModel(_storageServiceMock, _eventAggregatorMock, _errorHandlerServiceMock);
            Assert.IsNotNull(_viewModel);
        }

        [TestMethod]
        public void TestConstructorThrowsExceptionOnNullParameter()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new SoilN2OBreakdownSettingsViewModel(null, null, null));
        }

        [TestMethod]
        public void TestTotalCalculation()
        {
            Farm testFarm = new Farm();
            testFarm.AnnualSoilN2OBreakdown = new Table_15_Default_Soil_N2O_Emission_BreakDown_Provider();
            _mockStorageService.Setup(x => x.GetActiveFarm()).Returns(testFarm);
            _mockEventAggregator.Setup(x => x.GetEvent<ValidationPassOccurredEvent>().Publish(null));
            _viewModel = new SoilN2OBreakdownSettingsViewModel(_storageServiceMock, _eventAggregatorMock, _errorHandlerServiceMock);
            _viewModel.OnNavigatedTo(null);
            _viewModel.Data.January = 8;
            _viewModel.Data.February = 8;
            _viewModel.Data.March = 8;
            _viewModel.Data.April = 8;
            _viewModel.Data.May = 8;
            _viewModel.Data.June = 8;
            _viewModel.Data.July = 8;
            _viewModel.Data.August = 8;
            _viewModel.Data.September = 8;
            _viewModel.Data.October = 8;
            _viewModel.Data.November = 8;
            _viewModel.Data.December = 8;

            Assert.AreEqual(96, _viewModel.TotalUserEnteredPercentageOfAllMonths);

            double total = 0;
            foreach (Months month in Enum.GetValues(typeof(Months)))
            {
                total += this._viewModel.Data.MonthlyValues.GetValueByMonth(month);
            }
            Assert.AreEqual(96, total);
        }
    }
}
