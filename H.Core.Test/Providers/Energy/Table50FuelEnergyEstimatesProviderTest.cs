using H.Core.Enumerations;
using H.Core.Providers.Energy;
using Microsoft.Extensions.Logging;
using Moq;

#nullable disable

namespace H.Core.Test.Providers.Energy
{
    [TestClass]
    public class Table50FuelEnergyEstimatesProviderTest
    {
        #region Fields

        private ITable50FuelEnergyEstimatesProvider _provider = null!;

        #endregion

        #region Initialization

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _provider = new Table50FuelEnergyEstimatesProvider();
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }
        #endregion

        #region Tests

        [TestMethod]
        public void GetFuelEnergyValue()
        {
            Table50FuelEnergyEstimatesData data = _provider.GetFuelEnergyEstimatesDataInstance(Province.NewBrunswick, SoilFunctionalCategory.EasternCanada, TillageType.NoTill, CropType.Potatoes);
            Assert.AreEqual(1.9, data.FuelEstimate);

            data = _provider.GetFuelEnergyEstimatesDataInstance(Province.BritishColumbia, SoilFunctionalCategory.Black,
                TillageType.NoTill, CropType.Oats);
            Assert.AreEqual(1.43, data.FuelEstimate);

            data = _provider.GetFuelEnergyEstimatesDataInstance(Province.BritishColumbia, SoilFunctionalCategory.Brown,
                TillageType.Reduced, CropType.Flax);
            Assert.AreEqual(1.78, data.FuelEstimate);

            data = _provider.GetFuelEnergyEstimatesDataInstance(Province.Alberta, SoilFunctionalCategory.Black,
                TillageType.Reduced, CropType.CrimsonCloverTrifoliumIncarnatum);
            Assert.AreEqual(2.39, data.FuelEstimate);

            data = _provider.GetFuelEnergyEstimatesDataInstance(Province.Saskatchewan, SoilFunctionalCategory.Brown,
                TillageType.Intensive, CropType.PigeonBean);
            Assert.AreEqual(2.02, data.FuelEstimate);
        }

        [TestMethod]
        public void GetFuelEnergyEstimateInstanceAlberta()
        {
            Table50FuelEnergyEstimatesData data = _provider.GetFuelEnergyEstimatesDataInstance(Province.Alberta, SoilFunctionalCategory.Black, TillageType.NoTill, CropType.CanarySeed);

            Assert.AreEqual(1.43, data.FuelEstimate);
        }

        [TestMethod]
        public void GetFuelEnergyEstimateInstanceOntario()
        {
            Table50FuelEnergyEstimatesData data = _provider.GetFuelEnergyEstimatesDataInstance(Province.Ontario, SoilFunctionalCategory.EasternCanada, TillageType.Reduced, CropType.WinterTurnipRapeBrassicaRapaSppOleiferaLCVLargo);
            Assert.AreEqual(1.8, data.FuelEstimate);
        }
        
        [TestMethod]
        public void GetFuelEnergyEstimateInstanceBritishColumbia()
        {
            Table50FuelEnergyEstimatesData data = _provider.GetFuelEnergyEstimatesDataInstance(Province.BritishColumbia, SoilFunctionalCategory.Brown, TillageType.NoTill, CropType.ForageForSeed);
            Assert.AreEqual(1.42, data.FuelEstimate);
        }

        [TestMethod]
        public void GetFuelEnergyEstimateInstanceNewfoundLand()
        {
            Table50FuelEnergyEstimatesData data = _provider.GetFuelEnergyEstimatesDataInstance(Province.Newfoundland, SoilFunctionalCategory.EasternCanada, TillageType.Intensive, CropType.Chickpeas);
            Assert.AreEqual(3.29, data.FuelEstimate);
        }

        [TestMethod]
        public void GetFuelEnergyInstanceWrongProvince()
        {
            Table50FuelEnergyEstimatesData data = _provider.GetFuelEnergyEstimatesDataInstance(Province.Yukon, SoilFunctionalCategory.Brown, TillageType.NoTill, CropType.ForageForSeed);
            Assert.AreEqual(0, data.FuelEstimate);
        }

        [TestMethod]
        public void GetFuelEnergyInstanceWrongSoil()
        {
            Table50FuelEnergyEstimatesData data = _provider.GetFuelEnergyEstimatesDataInstance(Province.Newfoundland, SoilFunctionalCategory.Organic, TillageType.NoTill, CropType.Durum);
            Assert.AreEqual(0, data.FuelEstimate);
        }

        [TestMethod]
        public void GetFuelEnergyInstanceWrongCrop()
        {
            Table50FuelEnergyEstimatesData data = _provider.GetFuelEnergyEstimatesDataInstance(Province.Newfoundland, SoilFunctionalCategory.EasternCanada, TillageType.NoTill, CropType.LargeKabuliChickpea);
            Assert.AreEqual(0, data.FuelEstimate);
        }

        [TestMethod]
        public void GetFuelEnergyEstimatesDataInstance_ReturnsData_WhenExactMatchExists()
        {
            // Arrange
            var provider = new Table50FuelEnergyEstimatesProvider();
            var province = Province.Alberta;
            var soilCategory = SoilFunctionalCategory.Brown;
            var tillageType = TillageType.Intensive;
            var cropType = CropType.Barley;

            // Act
            var result = provider.GetFuelEnergyEstimatesDataInstance(province, soilCategory, tillageType, cropType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(province, result.Province);
            Assert.AreEqual(soilCategory.GetSimplifiedSoilCategory(), result.SoilFunctionalCategory);
            Assert.AreEqual(tillageType, result.TillageType);
            Assert.AreEqual(cropType, result.CropType);
        }

        [TestMethod]
        public void GetFuelEnergyEstimatesDataInstance_ReturnsEmpty_WhenNoMatch()
        {
            // Arrange
            var provider = new Table50FuelEnergyEstimatesProvider();
            var province = (Province)999;
            var soilCategory = (SoilFunctionalCategory)999;
            var tillageType = (TillageType)999;
            var cropType = (CropType)999;

            // Act
            var result = provider.GetFuelEnergyEstimatesDataInstance(province, soilCategory, tillageType, cropType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(default(Province), result.Province);
            Assert.AreEqual(default(SoilFunctionalCategory), result.SoilFunctionalCategory);
            Assert.AreEqual(default(TillageType), result.TillageType);
            Assert.AreEqual(default(CropType), result.CropType);
            Assert.AreEqual(0.0, result.FuelEstimate);
        }

        [TestMethod]
        public void GetFuelEnergyEstimatesDataInstance_LogsError_WhenCropTypeNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var provider = new Table50FuelEnergyEstimatesProvider(loggerMock.Object);
            var province = Province.Alberta;
            var soilCategory = SoilFunctionalCategory.Brown;
            var tillageType = TillageType.Intensive;
            var cropType = (CropType)999;

            // Act
            var result = provider.GetFuelEnergyEstimatesDataInstance(province, soilCategory, tillageType, cropType);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("unable to find Crop")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.AtLeastOnce);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetFuelEnergyEstimatesDataInstance_FallowCropType_UsesFallow()
        {
            // Arrange
            var provider = new Table50FuelEnergyEstimatesProvider();
            var province = Province.Alberta;
            var soilCategory = SoilFunctionalCategory.Brown;
            var tillageType = TillageType.Intensive;
            var cropType = CropType.SummerFallow; // Should be mapped to Fallow

            // Act
            var result = provider.GetFuelEnergyEstimatesDataInstance(province, soilCategory, tillageType, cropType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(CropType.Fallow, result.CropType);
        }
        #endregion
    }
}
