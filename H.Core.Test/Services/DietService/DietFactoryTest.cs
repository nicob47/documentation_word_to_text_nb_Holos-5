using H.Core.Enumerations;
using H.Core.Properties;
using H.Core.Providers.Feed;
using H.Core.Services.DietService;
using H.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace H.Core.Test.Services.DietService;

[TestClass]
public class DietFactoryTest
{
    #region Fields

    private IDietFactory _sut = null!;

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
        var mockCacheService = new Mock<ICacheService>();
        var mockFeedIngredientProvider = new Mock<IFeedIngredientProvider>();

        mockFeedIngredientProvider.Setup(x => x.GetIngredientsForDiet(It.IsAny<AnimalType>(), It.IsAny<DietType>()))
            .Returns(new List<IFeedIngredient>());

        _sut = new DietFactory(mockLogger.Object, mockCacheService.Object, mockFeedIngredientProvider.Object);
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }

    #endregion

    #region Tests

    [TestMethod]
    public void CreateReturnsNonEmptyDiet()
    {
        var result = _sut.Create(DietType.LowEnergyAndProtein, AnimalType.BeefCow);

        Assert.AreEqual(Resources.LowEnergyProtein, result.Name);
    }

    [TestMethod]
    public void GetValidDietsReturnsNonZeroCount()
    {
        var result = _sut.GetValidDietKeys();

        Assert.IsTrue(result.Count > 0);
    }

    [TestMethod]
    public void Create_Parameterless_ThrowsNotImplementedException()
    {
        Assert.ThrowsExactly<NotImplementedException>(() => _sut.Create());
    }

    [TestMethod]
    public void IsValidDietType_ReturnsTrue_ForValidCombination()
    {
        var isValid = _sut.IsValidDietType(AnimalType.BeefCow, DietType.LowEnergyAndProtein);
        Assert.IsTrue(isValid);
    }

    [TestMethod]
    public void IsValidDietType_ReturnsFalse_ForInvalidCombination()
    {
        var isValid = _sut.IsValidDietType(AnimalType.Sheep, DietType.HighEnergyAndProtein);
        Assert.IsFalse(isValid);
    }

    [TestMethod]
    public void Create_ReturnsUnknownDiet_ForInvalidCombination()
    {
        var result = _sut.Create(DietType.HighEnergyAndProtein, AnimalType.Sheep);
        Assert.AreEqual("Unknown diet", result.Name);
    }

    /// <summary>
    /// Regression test: sheep GoodQualityForage diet must carry a forage percentage of 85%.
    /// With the old bug (Forage = 0), CalculateVolatileSolids would incorrectly use
    /// urinaryEnergy = 0.02 instead of 0.04 because grain-in-diet would equal 100.
    /// </summary>
    [TestMethod]
    public void Create_SheepGoodQualityForage_HasCorrectForagePercentage()
    {
        var diet = _sut.Create(DietType.GoodQualityForage, AnimalType.Sheep);

        Assert.AreEqual(85, diet.Forage);
    }

    /// <summary>
    /// Regression test: sheep AverageQualityForage diet must carry a forage percentage of 97%.
    /// </summary>
    [TestMethod]
    public void Create_SheepAverageQualityForage_HasCorrectForagePercentage()
    {
        var diet = _sut.Create(DietType.AverageQualityForage, AnimalType.Sheep);

        Assert.AreEqual(97, diet.Forage);
    }

    /// <summary>
    /// Regression test: sheep PoorQualityForage diet must carry a forage percentage of 100%.
    /// </summary>
    [TestMethod]
    public void Create_SheepPoorQualityForage_HasCorrectForagePercentage()
    {
        var diet = _sut.Create(DietType.PoorQualityForage, AnimalType.Sheep);

        Assert.AreEqual(100, diet.Forage);
    }

    [TestMethod]
    public void Create_ReturnsAllValidDiets_FromDietCollection()
    {
        // Arrange
        var validKeys = _sut.GetValidDietKeys();

        // Act
        var createdDiets = validKeys
            .Select(key => _sut.Create(key.Item2, key.Item1))
            .ToList();

        // Assert
        Assert.AreEqual(validKeys.Count, createdDiets.Count);
        foreach (var (animalType, dietType) in validKeys)
        {
            var diet = createdDiets.SingleOrDefault(d => d.AnimalType == animalType && d.DietType == dietType);
            Assert.IsNotNull(diet, $"Diet not found for AnimalType: {animalType}, DietType: {dietType}");
            Assert.AreEqual(animalType, diet.AnimalType);
            Assert.AreEqual(dietType, diet.DietType);
        }
    }

    #endregion
}
