using H.Avalonia.ViewModels.OptionsViews.DataTransferObjects;
using H.Core;
using H.Core.Models;
using H.Core.Services.StorageService;
using Moq;

namespace H.Avalonia.Test.ViewModels.OptionsViews.DataTransferObjects
{
    [TestClass]
    public class FarmSettingsDTOTests
    {
        private Mock<IStorageService> _mockStorageService = null!;
        private IStorageService _storageServiceMock = null!;
        private Mock<IStorage> _mockStorage = null!;
        private IStorage _storageMock = null!;
        private ApplicationData _applicationData = null!;
        private FarmSettingsDTO _farmSettingsDTO = null!;

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
            _farmSettingsDTO = new FarmSettingsDTO(_storageServiceMock);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void TestValidateStringCorrectInput()
        {
            _farmSettingsDTO.Name = "NewName";
            Assert.IsTrue(!_farmSettingsDTO.HasErrors);
        }

        [TestMethod]
        public void TestValidateStringBadInput()
        {
            Assert.IsTrue(!_farmSettingsDTO.HasErrors);

            _farmSettingsDTO.FarmName = "";

            Assert.IsTrue(_farmSettingsDTO.HasErrors);
            var errors = _farmSettingsDTO.GetErrors(nameof(_farmSettingsDTO.FarmName)) as IEnumerable<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual(H.Core.Properties.Resources.ErrorNameCannotBeEmpty, errors.ToList()[0]);
            Assert.AreEqual(_farmSettingsDTO.FarmName, "TestFarm");
        }

        [TestMethod]
        public void TestValidateNonNegativeCorrectInput()
        {
            _farmSettingsDTO.GrowingSeasonPrecipitation = 1.11;
            Assert.IsTrue(!_farmSettingsDTO.HasErrors);
        }

        [TestMethod]
        public void TestValidateNonNegativeBadInput()
        {
            _farmSettingsDTO.GrowingSeasonPrecipitation = 2.05;
            Assert.IsTrue(!_farmSettingsDTO.HasErrors);

            _farmSettingsDTO.GrowingSeasonPrecipitation = -2.99;

            Assert.IsTrue(_farmSettingsDTO.HasErrors);
            var errors = _farmSettingsDTO.GetErrors(nameof(_farmSettingsDTO.GrowingSeasonPrecipitation)) as IEnumerable<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual(H.Core.Properties.Resources.ErrorMustBeGreaterThan0, errors.ToList()[0]);
            Assert.AreEqual(_farmSettingsDTO.GrowingSeasonPrecipitation, 2.05);
        }
    }
}
