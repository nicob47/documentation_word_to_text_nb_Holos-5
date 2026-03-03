using H.Avalonia.ViewModels.ComponentViews.OtherAnimals;
using H.Core;
using H.Core.Enumerations;
using H.Core.Models;
using H.Core.Models.Animals;
using H.Core.Models.Animals.OtherAnimals;
using H.Core.Services.Animals;
using H.Core.Services.StorageService;
using Microsoft.Extensions.Logging;
using Moq;

#nullable disable

namespace H.Avalonia.Test.ViewModels.ComponentViews.OtherAnimals
{
    [TestClass]
    public class HorsesComponentViewModelTests
    {
        private HorsesComponentViewModel _viewModel = null!;
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

            _viewModel = new HorsesComponentViewModel(mockLogger.Object, _mockAnimalComponentService.Object, _storageServiceMock, mockManagementPeriodService.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void TestConstructorSettingViewName()
        {
            string expectedName = "Horses";
            Assert.AreEqual(expectedName, _viewModel.ViewName);
        }

        [TestMethod]
        public void TestConstructorSettingAnimalType()
        {
            AnimalType expectedAnimalType = AnimalType.Horses;
            Assert.AreEqual(expectedAnimalType, _viewModel.AnimalType);
        }

        // Below we are testing methods found in OtherAnimalsViewModelBase (abstract) used by all child classes (horses, bison, goats, etc.)

        [TestMethod]
        public void TestConstructorInitializingCollections()
        {
            Assert.IsNotNull(_viewModel.ManagementPeriodDtos);
            Assert.AreEqual(0, _viewModel.ManagementPeriodDtos.Count);
            Assert.IsNotNull(_viewModel.Groups);
            Assert.AreEqual(0, _viewModel.Groups.Count);
        }

        [TestMethod]
        public void TestHandleAddGroupEvent()
        {
            AnimalType expectedGroupType = AnimalType.NotSelected;

            _viewModel.OnAddAnimalGroupDtoCommandExecute();

            Assert.AreEqual(1, _viewModel.AnimalGroupDtos.Count);
            Assert.AreEqual(expectedGroupType, _viewModel.AnimalGroupDtos[0].GroupType);
        }

        [TestMethod]
        public void TestHandleAddManagementPeriodEvent()
        {
            string expectedPeriodName = "Period #1";
            DateTime expectedStart = new DateTime(2024, 01, 01);
            DateTime expectedEnd = new DateTime(2025, 01, 01);
            int expectedDays = 364;

            _viewModel.OnAddManagementPeriodExecute();

            Assert.AreEqual(1, _viewModel.ManagementPeriodDtos.Count);
            Assert.AreEqual(expectedPeriodName, _viewModel.ManagementPeriodDtos[0].Name);
            Assert.AreEqual(expectedStart, _viewModel.ManagementPeriodDtos[0].Start);
            Assert.AreEqual(expectedEnd, _viewModel.ManagementPeriodDtos[0].End);
            Assert.AreEqual(expectedDays, _viewModel.ManagementPeriodDtos[0].NumberOfDays);
        }

        [TestMethod]
        public void TestAddExistingManagementPeriods()
        {
            var testFarm = new Farm();
            var testHorsesComponent = new HorsesComponent();
            var testGroup = new AnimalGroup();
            var testManagementPeriod = new ManagementPeriod() { GroupName = "Period #0", Start = new DateTime(2020, 01, 01), End = new DateTime(2020, 03, 13), NumberOfDays = 72 };
            testGroup.ManagementPeriods.Add(testManagementPeriod);
            testHorsesComponent.Groups.Add(testGroup);
            testFarm.Components.Add(testHorsesComponent);
            _mockStorageService.Setup(x => x.GetActiveFarm()).Returns(testFarm);
            
            
           _viewModel.AddExistingManagementPeriods();

            Assert.AreEqual(testManagementPeriod.GroupName, _viewModel.ManagementPeriodDtos[0].Name);
            Assert.AreEqual(testManagementPeriod.Start, _viewModel.ManagementPeriodDtos[0].Start);
            Assert.AreEqual(testManagementPeriod.End, _viewModel.ManagementPeriodDtos[0].End);
            Assert.AreEqual(testManagementPeriod.NumberOfDays, _viewModel.ManagementPeriodDtos[0].NumberOfDays);
        }

        [TestMethod]
        public void TestValidateViewName()
        {
            Assert.IsFalse(_viewModel.HasErrors);

            _viewModel.ViewName = "";

            Assert.IsTrue(_viewModel.HasErrors);
            var errors = _viewModel.GetErrors(nameof(_viewModel.ViewName)) as IEnumerable<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual("Name cannot be empty.", errors.ToList()[0]);
        }
    }
}
