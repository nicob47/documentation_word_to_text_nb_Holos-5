using H.Avalonia.ViewModels.ComponentViews.OtherAnimals;
using H.Core;
using H.Core.Enumerations;
using H.Core.Models;
using H.Core.Services.Animals;
using H.Core.Services.StorageService;
using Microsoft.Extensions.Logging;
using Moq;

namespace H.Avalonia.Test.ViewModels.ComponentViews.OtherAnimals
{
    [TestClass]
    public class DeerComponentViewModelTests
    {
        private DeerComponentViewModel _viewModel = null!;
        private Mock<IStorageService> _mockStorageService = null!;
        private IStorageService _storageServiceMock = null!;
        private Mock<IStorage> _mockStorage = null!;
        private IStorage _storageMock = null!;
        private ApplicationData _applicationData = null!;
        private Mock<IAnimalComponentService> _mockAnimalComponentService = null!;

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
            var mockLogger = new Mock<ILogger>();
            _mockAnimalComponentService = new Mock<IAnimalComponentService>();
            var mockManagementPeriodService = new Mock<IManagementPeriodService>();

            _viewModel = new DeerComponentViewModel(mockLogger.Object, _mockAnimalComponentService.Object, _storageServiceMock, mockManagementPeriodService.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void TestConstructorSettingViewName()
        {
            string expectedName = "Deer";
            Assert.AreEqual(expectedName, _viewModel.ViewName);
        }

        [TestMethod]
        public void TestConstructorSettingAnimalType()
        {
            AnimalType expectedAnimalType = AnimalType.Deer;
            Assert.AreEqual(expectedAnimalType, _viewModel.AnimalType);
        }
    }
}
