#region Imports

using AutoMapper;
using H.Core.Enumerations;
using H.Core.Mappers;
using H.Core.Models;
using H.Core.Providers.Feed;
using H.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Prism.Ioc;

#nullable disable

#endregion

namespace H.Core.Test.Providers.Feed {
    [TestClass]
    public class FeedIngredientProviderTest {

        #region Fields

        private FeedIngredientProvider _sut = null!;

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
            var mockLogger = new Mock<ILogger>();
            var mockContainerProvider = new Mock<IContainerProvider>();
            var mockCacheService = new Mock<ICacheService>();

            mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), It.IsAny<string>())).Returns(new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<FeedIngredientToFeedIngredientMapper>();
            }).CreateMapper());

            _sut = new FeedIngredientProvider(mockLogger.Object, mockContainerProvider.Object, mockCacheService.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        #endregion

        #region Tests

        [TestMethod]
        public void GetFeedDataReturnsCorrectNumberOfRows()
        {
            var result = _sut.GetBeefFeedIngredients();

            Assert.AreEqual(218, result.Count());
        }

        [TestMethod]
        public void GetFeedDataReturnsCorrectlyParseRow()
        {
            var feedData = _sut.GetBeefFeedIngredients();
            var item = feedData.Single(x => x.IngredientType == IngredientType.AlfalfaCubes);

            Assert.AreEqual(100, item.Forage);
            Assert.AreEqual(91, item.DryMatter);
            Assert.AreEqual(18.1, item.CrudeProtein);
            Assert.AreEqual(39.3, item.SP);
            Assert.AreEqual(8.2, item.ADICP);
            Assert.AreEqual(0, item.Sugars);
            Assert.AreEqual(0, item.OA);
            Assert.AreEqual(2.1, item.Fat);
            Assert.AreEqual(12, item.Ash);
            Assert.AreEqual(1.3, item.Starch);
            Assert.AreEqual(45.5, item.NDF);
            Assert.AreEqual(7.6, item.Lignin);
            Assert.AreEqual(56, item.TotalDigestibleNutrient);
            Assert.AreEqual(2, item.ME);
            Assert.AreEqual(1.2, item.NEma);
            Assert.AreEqual(0.6, item.NEga);
            Assert.AreEqual(31, item.RUP);
            Assert.AreEqual(5.1, item.kdPB);
            Assert.AreEqual(30, item.KdCB1);
            Assert.AreEqual(30, item.KdCB2);
            Assert.AreEqual(5.5, item.KdCB3);
            Assert.AreEqual(49.3, item.PBID);
            Assert.AreEqual(75, item.CB1ID);
            Assert.AreEqual(75, item.CB2ID);
            Assert.AreEqual(92, item.Pef);
            Assert.AreEqual(1.16, item.ARG);
            Assert.AreEqual(0.47, item.HIS);
            Assert.AreEqual(1.09, item.ILE);
            Assert.AreEqual(1.67, item.LEU);
            Assert.AreEqual(1.09, item.LYS);
            Assert.AreEqual(0.13, item.MET);
            Assert.AreEqual(0, item.CYS);
            Assert.AreEqual(1.14, item.PHE);
            Assert.AreEqual(0, item.TYR);
            Assert.AreEqual(0.9, item.THR);
            Assert.AreEqual(0.33, item.TRP);
            Assert.AreEqual(1.29, item.VAL);
            Assert.AreEqual(1.49, item.Ca);
            Assert.AreEqual(0.28, item.P);
            Assert.AreEqual(0.28, item.Mg);
            Assert.AreEqual(0.7, item.Cl);
            Assert.AreEqual(2.05, item.K);
            Assert.AreEqual(0.16, item.Na);
            Assert.AreEqual(0.25, item.S);
            Assert.AreEqual(0.77, item.Co);
            Assert.AreEqual(8.54, item.Cu);
            Assert.AreEqual(0, item.I);
            Assert.AreEqual(648.5, item.Fe);
            Assert.AreEqual(44.1, item.Mn);
            Assert.AreEqual(0.8, item.Se);
            Assert.AreEqual(24.3, item.Zn);
            Assert.AreEqual(19.3, item.VitA);
            Assert.AreEqual(1, item.VitD);
            Assert.AreEqual(0, item.VitE);
        }

        [TestMethod]
        public void GetDairyFeedIngredientsReturnsCorrectlyParseRow()
        {
            var feedData = _sut.GetDairyFeedIngredients();
            var item = feedData.Single(x => x.IngredientType == IngredientType.AlfalfaMedicago);
            Assert.AreEqual(56.4, item.TotalDigestibleNutrient);
            Assert.AreEqual(DairyFeedClassType.Forage, item.DairyFeedClass);
            Assert.AreEqual(2.6, item.DE);
            Assert.AreEqual(1.96, item.ME);
            Assert.AreEqual(1.19, item.NEL_ThreeX);
            Assert.AreEqual(1.11, item.NEL_FourX);
            Assert.AreEqual(1.27, item.NEM);
            Assert.AreEqual(0.7, item.NEG);
            Assert.AreEqual(90.3, item.DryMatter);
            Assert.AreEqual(19.2, item.CrudeProtein);
            Assert.AreEqual(3.1, item.NDICP);
            Assert.AreEqual(2.4, item.ADICP);
            Assert.AreEqual(2.5, item.EE);
            Assert.AreEqual(41.6, item.NDF);
            Assert.AreEqual(32.8, item.ADF);
            Assert.AreEqual(7.6, item.Lignin);
            Assert.AreEqual(11, item.Ash);
        }

        [TestMethod]
        public void GetSwineFeedIngredientsReturnsCorrectlyParseRow()
        {
            var feedData = _sut.GetSwineFeedIngredients();
            var item = feedData.Single(x => x.IngredientType == IngredientType.AlfalfaHay);
            var item1 = feedData.Single(x => x.IngredientType == IngredientType.YeastTorula);
            var item2 = feedData.Single(x => x.IngredientType == IngredientType.WheatShorts);

            Assert.AreEqual(77, item2.IleDigestAID);
            Assert.AreEqual("96.7", item1.AAFCO);
            Assert.AreEqual("3.1", item.AAFCO);
            Assert.AreEqual("P324", item.AAFCO2010);
            Assert.AreEqual("1-30-293", item.IFN);
            Assert.AreEqual(90.33, item.DryMatter);
            Assert.AreEqual(19.32, item.CrudeProtein);
            Assert.AreEqual(2.3, item.EE);
            Assert.AreEqual(11, item.Ash);
            Assert.AreEqual(4077, item.GrossEnergy);
            Assert.AreEqual(1830, item.DeSwine);
            Assert.AreEqual(1699, item.ME);
            Assert.AreEqual(878, item.NE);
            Assert.AreEqual(1.02, item.Starch);
            Assert.AreEqual(37, item.NDF);
            Assert.AreEqual(31.01, item.ADF);
            Assert.AreEqual(6.65, item.ADL);
            Assert.AreEqual(1.46, item.Ca);
            Assert.AreEqual(2.48, item.K);
            Assert.AreEqual(0, item.CrudeFiber);
            Assert.AreEqual(0, item.Lactose);
            Assert.AreEqual(0, item.IVP);
        }

        [TestMethod]
        public void GetSwineDataReturnsCorrectNumberOfRows()
        {
            var data = _sut.GetSwineFeedIngredients();
            Assert.AreEqual(123, data.Count());
        }

        [TestMethod]
        public void GetBeefDataReturnsCorrectNumberOfRows()
        {
            var data = _sut.GetBeefFeedIngredients();
            Assert.AreEqual(218, data.Count());
        }

        [TestMethod]
        public void GetDairyDataReturnsCorrectNumberOfRows()
        {
            var data = _sut.GetDairyFeedIngredients();
            Assert.AreEqual(122, data.Count());
        }

        [TestMethod]
        public void GetIngredientsForDiet_BeefCowLowEnergyAndProtein_ReturnsNativePrairieHay()
        {
            var result = _sut.GetIngredientsForDiet(AnimalType.BeefCow, DietType.LowEnergyAndProtein);
            Assert.AreEqual(1, result.Count);
            var ingredient = result.First();
            Assert.AreEqual(IngredientType.NativePrairieHay, ((FeedIngredient)ingredient).IngredientType);
            Assert.AreEqual(100, ingredient.PercentageInDiet);
        }

        [TestMethod]
        public void GetIngredientsForDiet_BeefCowMediumEnergyAndProtein_ReturnsExpectedIngredients()
        {
            var result = _sut.GetIngredientsForDiet(AnimalType.BeefCow, DietType.MediumEnergyAndProtein).ToList();
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(IngredientType.AlfalfaHay, ((FeedIngredient)result[0]).IngredientType);
            Assert.AreEqual(32, result[0].PercentageInDiet);
            Assert.AreEqual(IngredientType.MeadowHay, ((FeedIngredient)result[1]).IngredientType);
            Assert.AreEqual(65, result[1].PercentageInDiet);
            Assert.AreEqual(IngredientType.BarleyGrain, ((FeedIngredient)result[2]).IngredientType);
            Assert.AreEqual(3, result[2].PercentageInDiet);
        }

        [TestMethod]
        public void GetIngredientsForDiet_BeefCowHighEnergyAndProtein_ReturnsExpectedIngredients()
        {
            var result = _sut.GetIngredientsForDiet(AnimalType.BeefCow, DietType.HighEnergyAndProtein).ToList();
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(IngredientType.OrchardgrassHay, ((FeedIngredient)result[0]).IngredientType);
            Assert.AreEqual(60, result[0].PercentageInDiet);
            Assert.AreEqual(IngredientType.AlfalfaHay, ((FeedIngredient)result[1]).IngredientType);
            Assert.AreEqual(20, result[1].PercentageInDiet);
            Assert.AreEqual(IngredientType.BarleyGrain, ((FeedIngredient)result[2]).IngredientType);
            Assert.AreEqual(20, result[2].PercentageInDiet);
        }

        [TestMethod]
        public void GetIngredientsForDiet_InvalidAnimalType_ReturnsEmptyList()
        {
            var result = _sut.GetIngredientsForDiet(AnimalType.Sheep, DietType.LowEnergyAndProtein);

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void GetIngredientsForDiet_InvalidDietType_ReturnsEmptyList()
        {
            var result = _sut.GetIngredientsForDiet(AnimalType.BeefCow, (DietType)999);

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void CopyIngredient_CopiesValuesAndSetsPercentage()
        {
            var original = _sut.GetBeefFeedIngredients().First();
            var percentage = 42.5;
            var copy = _sut.CopyIngredient(original, percentage);

            Assert.AreEqual(original.IngredientType, copy.IngredientType);
            Assert.AreEqual(percentage, copy.PercentageInDiet);
            Assert.AreNotSame(original, copy);
        }

        [TestMethod]
        public void Get_ReturnsNull_WhenIngredientNotFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockContainerProvider = new Mock<IContainerProvider>();
            var mockCacheService = new Mock<ICacheService>();

            mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), It.IsAny<string>())).Returns(new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<FeedIngredientToFeedIngredientMapper>();
            }).CreateMapper());

            var provider = new FeedIngredientProvider(mockLogger.Object, mockContainerProvider.Object, mockCacheService.Object);

            // Act
            var method = typeof(FeedIngredientProvider).GetMethod("Get", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method.Invoke(provider, new object[] { ComponentCategory.BeefProduction, (IngredientType)9999 });

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void CopyIngredient_ReturnsNull_WhenIngredientIsNull()
        {
            var result = _sut.CopyIngredient(null, 10.0);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetIngredient_ReturnsNull_WhenIngredientNotFound()
        {
            var method = typeof(FeedIngredientProvider).GetMethod("GetIngredient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method.Invoke(_sut, new object[] { (IngredientType)9999, 10.0, ComponentCategory.BeefProduction });
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Dictionaries_ArePopulated_AfterConstruction()
        {
            var ingredientsByAnimalCategory = typeof(FeedIngredientProvider)
                .GetField("_ingredientsByAnimalCategory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(_sut) as Dictionary<ComponentCategory, IReadOnlyList<IFeedIngredient>>;
            var ingredientDictionary = typeof(FeedIngredientProvider)
                .GetField("_ingredientDictionary", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(_sut) as Dictionary<Tuple<ComponentCategory, IngredientType>, IFeedIngredient>;

            Assert.IsTrue(ingredientsByAnimalCategory.Count > 0);
            Assert.IsTrue(ingredientDictionary.Count > 0);
        }

        #endregion

        #region BeefFinisher Diet Unit Tests

        [TestMethod]
        public void GetIngredientsForDiet_BeefFinisher_BarleyGrainBased_ReturnsCorrectIngredients()
        {
            // Arrange
            var provider = _sut;

            // Act
            var result = provider.GetIngredientsForDiet(AnimalType.BeefFinisher, DietType.BarleyGrainBased).ToList();

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(IngredientType.BarleySilage, result[0].IngredientType);
            Assert.AreEqual(10, result[0].PercentageInDiet);
            Assert.AreEqual(IngredientType.BarleyGrain, result[1].IngredientType);
            Assert.AreEqual(90, result[1].PercentageInDiet);
        }

        [TestMethod]
        public void GetIngredientsForDiet_BeefFinisher_CornGrainBased_ReturnsCorrectIngredients()
        {
            // Arrange
            var provider = _sut;

            // Act
            var result = provider.GetIngredientsForDiet(AnimalType.BeefFinisher, DietType.CornGrainBased).ToList();

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(IngredientType.BarleySilage, result[0].IngredientType);
            Assert.AreEqual(10, result[0].PercentageInDiet);
            Assert.AreEqual(IngredientType.CornGrain, result[1].IngredientType);
            Assert.AreEqual(88.7, result[1].PercentageInDiet);
            Assert.AreEqual(IngredientType.Urea, result[2].IngredientType);
            Assert.AreEqual(1.3, result[2].PercentageInDiet);
        }

        #endregion

        #region BeefBackgrounder Diet Unit Tests

        [TestMethod]
        public void GetIngredientsForDiet_BeefBackgrounder_SlowGrowth_ReturnsCorrectIngredients()
        {
            // Arrange
            var provider = _sut;

            // Act
            var result = provider.GetIngredientsForDiet(AnimalType.BeefBackgrounder, DietType.SlowGrowth).ToList();

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(IngredientType.BarleySilage, result[0].IngredientType);
            Assert.AreEqual(65, result[0].PercentageInDiet);
            Assert.AreEqual(IngredientType.CornGrain, result[1].IngredientType);
            Assert.AreEqual(35, result[1].PercentageInDiet);
        }

        [TestMethod]
        public void GetIngredientsForDiet_BeefBackgrounder_MediumGrowth_ReturnsCorrectIngredients()
        {
            // Arrange
            var provider = _sut;

            // Act
            var result = provider.GetIngredientsForDiet(AnimalType.BeefBackgrounder, DietType.MediumGrowth).ToList();

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(IngredientType.BarleySilage, result[0].IngredientType);
            Assert.AreEqual(65, result[0].PercentageInDiet);
            Assert.AreEqual(IngredientType.BarleyGrain, result[1].IngredientType);
            Assert.AreEqual(35, result[1].PercentageInDiet);
        }

        #endregion
        
        #region Swine Diet Unit Tests

        [TestMethod]
        public void GetIngredientsForDiet_Swine_Gestation_ReturnsCorrectIngredients()
        {
            var result = _sut.GetIngredientsForDiet(AnimalType.Swine, DietType.Gestation).ToList();
            Assert.AreEqual(8, result.Count);
            Assert.AreEqual(IngredientType.WheatBran, result[0].IngredientType);
            Assert.AreEqual(14, result[0].PercentageInDiet);
            Assert.AreEqual(IngredientType.WheatShorts, result[1].IngredientType);
            Assert.AreEqual(3, result[1].PercentageInDiet);
            Assert.AreEqual(IngredientType.Barley, result[2].IngredientType);
            Assert.AreEqual(62.2, result[2].PercentageInDiet);
            Assert.AreEqual(IngredientType.SoybeanMealDehulledExpelled, result[3].IngredientType);
            Assert.AreEqual(4, result[3].PercentageInDiet);
            Assert.AreEqual(IngredientType.CanolaMealExpelled, result[4].IngredientType);
            Assert.AreEqual(3, result[4].PercentageInDiet);
            Assert.AreEqual(IngredientType.FieldPeas, result[5].IngredientType);
            Assert.AreEqual(6, result[5].PercentageInDiet);
            Assert.AreEqual(IngredientType.SugarBeetPulp, result[6].IngredientType);
            Assert.AreEqual(5.6, result[6].PercentageInDiet);
            Assert.AreEqual(IngredientType.CanolaFullFat, result[7].IngredientType);
            Assert.AreEqual(0.4, result[7].PercentageInDiet);
        }

        [TestMethod]
        public void GetIngredientsForDiet_Swine_Lactation_ReturnsCorrectIngredients()
        {
            var result = _sut.GetIngredientsForDiet(AnimalType.Swine, DietType.Lactation).ToList();
            Assert.AreEqual(7, result.Count);
            Assert.AreEqual(IngredientType.WheatBran, result[0].IngredientType);
            Assert.AreEqual(41, result[0].PercentageInDiet);
            Assert.AreEqual(IngredientType.Barley, result[1].IngredientType);
            Assert.AreEqual(21.8, result[1].PercentageInDiet);
            Assert.AreEqual(IngredientType.CornDistillersDriedGrainsSolublesGreaterThanSixAndLessThanNinePercentOil, result[2].IngredientType);
            Assert.AreEqual(9, result[2].PercentageInDiet);
            Assert.AreEqual(IngredientType.SoybeanMealDehulledExpelled, result[3].IngredientType);
            Assert.AreEqual(9, result[3].PercentageInDiet);
            Assert.AreEqual(IngredientType.CanolaMealExpelled, result[4].IngredientType);
            Assert.AreEqual(5, result[4].PercentageInDiet);
            Assert.AreEqual(IngredientType.FieldPeas, result[5].IngredientType);
            Assert.AreEqual(10, result[5].PercentageInDiet);
            Assert.AreEqual(IngredientType.CanolaFullFat, result[6].IngredientType);
            Assert.AreEqual(1.8, result[6].PercentageInDiet);
        }

        [TestMethod]
        public void GetIngredientsForDiet_Swine_NurseryWeanersStarter1_ReturnsCorrectIngredients()
        {
            var result = _sut.GetIngredientsForDiet(AnimalType.Swine, DietType.NurseryWeanersStarter1).ToList();
            Assert.AreEqual(7, result.Count);
            Assert.AreEqual(IngredientType.WheatBran, result[0].IngredientType);
            Assert.AreEqual(39, result[0].PercentageInDiet);
            Assert.AreEqual(IngredientType.CornDistillersDriedGrainsSolublesGreaterThanSixAndLessThanNinePercentOil, result[1].IngredientType);
            Assert.AreEqual(11.38, result[1].PercentageInDiet);
            Assert.AreEqual(IngredientType.SoybeanMealDehulledExpelled, result[2].IngredientType);
            Assert.AreEqual(20, result[2].PercentageInDiet);
            Assert.AreEqual(IngredientType.FieldPeas, result[3].IngredientType);
            Assert.AreEqual(11, result[3].PercentageInDiet);
            Assert.AreEqual(IngredientType.CanolaFullFat, result[4].IngredientType);
            Assert.AreEqual(1.2, result[4].PercentageInDiet);
            Assert.AreEqual(IngredientType.WheyPermeateLactose80, result[5].IngredientType);
            Assert.AreEqual(10, result[5].PercentageInDiet);
            Assert.AreEqual(IngredientType.FishMealCombined, result[6].IngredientType);
            Assert.AreEqual(5, result[6].PercentageInDiet);
        }

        [TestMethod]
        public void GetIngredientsForDiet_Swine_GrowerFinisherDiet1_ReturnsCorrectIngredients()
        {
            var result = _sut.GetIngredientsForDiet(AnimalType.Swine, DietType.GrowerFinisherDiet1).ToList();
            Assert.AreEqual(7, result.Count);
            Assert.AreEqual(IngredientType.WheatBran, result[0].IngredientType);
            Assert.AreEqual(32.53, result[0].PercentageInDiet);
            Assert.AreEqual(IngredientType.Barley, result[1].IngredientType);
            Assert.AreEqual(24, result[1].PercentageInDiet);
            Assert.AreEqual(IngredientType.CornDistillersDriedGrainsSolublesGreaterThanSixAndLessThanNinePercentOil, result[2].IngredientType);
            Assert.AreEqual(12, result[2].PercentageInDiet);
            Assert.AreEqual(IngredientType.SoybeanMealDehulledExpelled, result[3].IngredientType);
            Assert.AreEqual(10, result[3].PercentageInDiet);
            Assert.AreEqual(IngredientType.CanolaMealExpelled, result[4].IngredientType);
            Assert.AreEqual(6, result[4].PercentageInDiet);
            Assert.AreEqual(IngredientType.FieldPeas, result[5].IngredientType);
            Assert.AreEqual(12, result[5].PercentageInDiet);
            Assert.AreEqual(IngredientType.CanolaFullFat, result[6].IngredientType);
            Assert.AreEqual(1.2, result[6].PercentageInDiet);
        }

        #endregion

        #region Dairy Diet Unit Tests

        [TestMethod]
        public void GetIngredientsForDiet_DairyDryCow_CloseUp_ReturnsCorrectIngredients()
        {
            var result = _sut.GetIngredientsForDiet(AnimalType.DairyDryCow, DietType.CloseUp).ToList();
            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(IngredientType.CornYellowSilageNormal, result[0].IngredientType);
            Assert.AreEqual(48, result[0].PercentageInDiet);
            Assert.AreEqual(IngredientType.GrassLegumeMixturesPredomLegumesSilageMidMaturity, result[1].IngredientType);
            Assert.AreEqual(23, result[1].PercentageInDiet);
            Assert.AreEqual(IngredientType.CornYellowGrainCrackedDry, result[2].IngredientType);
            Assert.AreEqual(12, result[2].PercentageInDiet);
            Assert.AreEqual(IngredientType.CanolaMealMechExtracted, result[3].IngredientType);
            Assert.AreEqual(9, result[3].PercentageInDiet);
            Assert.AreEqual(IngredientType.CornYellowGlutenMealDried, result[4].IngredientType);
            Assert.AreEqual(8, result[4].PercentageInDiet);
        }

        [TestMethod]
        public void GetIngredientsForDiet_DairyHeifers_HighFiber_ReturnsCorrectIngredients()
        {
            var result = _sut.GetIngredientsForDiet(AnimalType.DairyHeifers, DietType.HighFiber).ToList();
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(IngredientType.GrassesCoolHayMature, result[0].IngredientType);
            Assert.AreEqual(50, result[0].PercentageInDiet);
            Assert.AreEqual(IngredientType.BarleyGrainRolled, result[1].IngredientType);
            Assert.AreEqual(45, result[1].PercentageInDiet);
            Assert.AreEqual(IngredientType.SoybeanMealExpellers, result[2].IngredientType);
            Assert.AreEqual(5, result[2].PercentageInDiet);
        }

        #endregion

        #region Caching Tests

        [TestMethod]
        public void GetIngredientsForDiet_ReturnsCachedResult_WhenPresent()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockContainerProvider = new Mock<IContainerProvider>();
            var mockCacheService = new Mock<ICacheService>();
            var expected = new List<IFeedIngredient> { new FeedIngredient { IngredientType = IngredientType.BarleyGrain, PercentageInDiet = 100 } };
            var cacheKey = "FeedIngredientProvider_GetIngredientsForDiet_BeefCow_LowEnergyAndProtein";

            mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), It.IsAny<string>())).Returns(new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<FeedIngredientToFeedIngredientMapper>();
            }).CreateMapper());

            mockCacheService.Setup(x => x.Get<IReadOnlyCollection<IFeedIngredient>>(cacheKey)).Returns(expected);

            var provider = new FeedIngredientProvider(mockLogger.Object, mockContainerProvider.Object, mockCacheService.Object);

            // Act
            var result = provider.GetIngredientsForDiet(AnimalType.BeefCow, DietType.LowEnergyAndProtein);

            // Assert
            Assert.AreSame(expected, result);
            mockCacheService.Verify(x => x.Get<IReadOnlyCollection<IFeedIngredient>>(cacheKey), Times.Once);
            mockCacheService.Verify(x => x.Set(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<IFeedIngredient>>(), It.IsAny<Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions>()), Times.Never);
        }

        [TestMethod]
        public void GetIngredientsForDiet_CachesResult_WhenNotCached()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockContainerProvider = new Mock<IContainerProvider>();
            var mockCacheService = new Mock<ICacheService>();
            var cacheKey = "FeedIngredientProvider_GetIngredientsForDiet_BeefCow_LowEnergyAndProtein";

            mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), It.IsAny<string>())).Returns(new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<FeedIngredientToFeedIngredientMapper>();
            }).CreateMapper());

            mockCacheService.Setup(x => x.Get<IReadOnlyCollection<IFeedIngredient>>(cacheKey)).Returns((IReadOnlyCollection<IFeedIngredient>)null);

            var provider = new FeedIngredientProvider(mockLogger.Object, mockContainerProvider.Object, mockCacheService.Object);

            // Act
            var result = provider.GetIngredientsForDiet(AnimalType.BeefCow, DietType.LowEnergyAndProtein);

            // Assert
            Assert.IsNotNull(result);
            mockCacheService.Verify(x => x.Get<IReadOnlyCollection<IFeedIngredient>>(cacheKey), Times.Once);
            mockCacheService.Verify(x => x.Set(cacheKey, result, It.IsAny<Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions>()), Times.Once);
        }

        #endregion
    }
}
