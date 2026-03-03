using H.Avalonia.ViewModels.OptionsViews.DataTransferObjects;
using H.Core;
using H.Core.Models;
using H.Core.Services.StorageService;
using Moq;

namespace H.Avalonia.Test.ViewModels.OptionsViews.DataTransferObjects
{

    [TestClass]
    public class SoilSettingsDTOTests
    {
        private Mock<IStorageService> _mockStorageService = null!;
        private IStorageService _storageServiceMock = null!;
        private Mock<IStorage> _mockStorage = null!;
        private IStorage _storageMock = null!;
        private ApplicationData _applicationData = null!;
        private SoilSettingsDTO _soilSettingsDTO = null!;

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
            _soilSettingsDTO = new SoilSettingsDTO(_storageServiceMock);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void TestValidateNonNegativeCorrectInput()
        {
            _soilSettingsDTO.BulkDensity = 1.87;
            Assert.IsTrue(!_soilSettingsDTO.HasErrors);
        }

        [TestMethod]
        public void TestValidateNonNegativeBadInput()
        {
            _soilSettingsDTO.BulkDensity = 4.56;
            Assert.IsTrue(!_soilSettingsDTO.HasErrors);

            _soilSettingsDTO.BulkDensity = -8.43;

            Assert.IsTrue(_soilSettingsDTO.HasErrors);
            Assert.AreEqual(4.56, _soilSettingsDTO.BulkDensity);
        }

        [TestMethod]
        public void TestValidateYearCorrectInput()
        {
            _soilSettingsDTO.CarbonModellingEquilibriumYear = 2011;
            Assert.IsTrue(!_soilSettingsDTO.HasErrors);
        }

        [TestMethod]
        public void TestValidateYearBadInput()
        {
            _soilSettingsDTO.CarbonModellingEquilibriumYear = 2007;
            Assert.IsTrue(!_soilSettingsDTO.HasErrors);

            _soilSettingsDTO.CarbonModellingEquilibriumYear = -1001;
            Assert.IsTrue (_soilSettingsDTO.HasErrors);
            Assert.AreEqual(2007, _soilSettingsDTO.CarbonModellingEquilibriumYear);
        }
    }
}
