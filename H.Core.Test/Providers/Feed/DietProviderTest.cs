#region Imports

using System.Linq;
using H.Core.Enumerations;
using H.Core.Providers.Feed;

#endregion

namespace H.Core.Test.Providers.Feed
{
    [TestClass]
    public class DietProviderTest
    {
        #region Fields

        private DietProvider _provider = null!;

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
            _provider = new DietProvider();
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        #endregion

        #region Tests

        /// <summary>
        /// Regression test: sheep GoodQualityForage diet must carry a forage percentage of 85%.
        /// With the old bug (Forage = 0), CalculateVolatileSolids would incorrectly use
        /// urinaryEnergy = 0.02 instead of 0.04 because grain-in-diet would equal 100.
        /// </summary>
        [TestMethod]
        public void GetDiets_SheepGoodQualityForage_HasCorrectForagePercentage()
        {
            var diets = _provider.GetDiets();

            var diet = diets.Single(x => x.AnimalType == AnimalType.Sheep && x.DietType == DietType.GoodQualityForage);

            Assert.AreEqual(85, diet.Forage);
        }

        /// <summary>
        /// Regression test: sheep AverageQualityForage diet must carry a forage percentage of 97%.
        /// </summary>
        [TestMethod]
        public void GetDiets_SheepAverageQualityForage_HasCorrectForagePercentage()
        {
            var diets = _provider.GetDiets();

            var diet = diets.Single(x => x.AnimalType == AnimalType.Sheep && x.DietType == DietType.AverageQualityForage);

            Assert.AreEqual(97, diet.Forage);
        }

        /// <summary>
        /// Regression test: sheep PoorQualityForage diet must carry a forage percentage of 100%.
        /// </summary>
        [TestMethod]
        public void GetDiets_SheepPoorQualityForage_HasCorrectForagePercentage()
        {
            var diets = _provider.GetDiets();

            var diet = diets.Single(x => x.AnimalType == AnimalType.Sheep && x.DietType == DietType.PoorQualityForage);

            Assert.AreEqual(100, diet.Forage);
        }

        #endregion
    }
}
