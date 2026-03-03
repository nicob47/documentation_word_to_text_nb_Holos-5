using H.Avalonia.ViewModels.OptionsViews.DataTransferObjects;
using H.Core;
using H.Core.Models;
using H.Core.Services.StorageService;
using Moq;

#nullable disable

namespace H.Avalonia.Test.ViewModels.OptionsViews.DataTransferObjects
{
    [TestClass]
    public class UserSettingsDTOTests
    {
        private Mock<IStorageService> _mockStorageService = null!;
        private IStorageService _storageServiceMock = null!;
        private Mock<IStorage> _mockStorage = null!;
        private IStorage _storageMock = null!;
        private ApplicationData _applicationData = null!;
        private UserSettingsDTO _userSettingsDTO = null!;

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
            var activeFarm = new Farm() { Name = "TestFarm" };
            _mockStorageService.Setup(x => x.GetActiveFarm()).Returns(activeFarm);
            _userSettingsDTO = new UserSettingsDTO(_storageServiceMock);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void TestValidateNonNegativeCorrectInput()
        {
            _userSettingsDTO.CustomN2OEmissionFactor = 9.86;
            Assert.IsTrue(!_userSettingsDTO.HasErrors);
        }

        [TestMethod]
        public void TestValidateNonNegativeBadInput()
        {
            _userSettingsDTO.CustomN2OEmissionFactor = 4.55;
            Assert.IsTrue(!_userSettingsDTO.HasErrors);

            _userSettingsDTO.CustomN2OEmissionFactor = -8.27;

            Assert.IsTrue(_userSettingsDTO.HasErrors);
            Assert.AreEqual(4.55, _userSettingsDTO.ActiveFarm.Defaults.CustomN2OEmissionFactor);
        }

        [TestMethod]
        public void TestValidatePercentageOver100()
        {
            _userSettingsDTO.PercentageOfStrawReturnedToSoilForRootCrops = 55.42;
            Assert.IsTrue(!_userSettingsDTO.HasErrors);

            _userSettingsDTO.PercentageOfStrawReturnedToSoilForRootCrops = 134.60;

            Assert.IsTrue(_userSettingsDTO.HasErrors);
            Assert.AreEqual(55.42, _userSettingsDTO.ActiveFarm.Defaults.PercentageOfStrawReturnedToSoilForRootCrops);
        }

        [TestMethod]
        public void TestValidatePercentageUnder0()
        {
            _userSettingsDTO.PercentageOfStrawReturnedToSoilForRootCrops = 66.51;
            Assert.IsTrue(!_userSettingsDTO.HasErrors);

            _userSettingsDTO.PercentageOfStrawReturnedToSoilForRootCrops = -9.87;

            Assert.IsTrue(_userSettingsDTO.HasErrors);
            Assert.AreEqual(66.51, _userSettingsDTO.ActiveFarm.Defaults.PercentageOfStrawReturnedToSoilForRootCrops);
        }

        [TestMethod]
        public void TestValidatePercentageCorrectInput()
        {
            _userSettingsDTO.PercentageOfStrawReturnedToSoilForRootCrops = 74.98;
            Assert.IsTrue(!_userSettingsDTO.HasErrors);
        }
    }
}
