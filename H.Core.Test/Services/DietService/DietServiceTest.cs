using H.Core.Enumerations;
using H.Core.Providers.Feed;
using H.Core.Services.DietService;
using Microsoft.Extensions.Logging;
using Moq;

namespace H.Core.Test.Services.DietService;

[TestClass]
public class DietServiceTest
{
    #region Fields

    private IDietService _sut = null!;
    private Mock<IDietFactory> _mockDietFactory = null!;

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
        var mockDietProvider = new Mock<IDietProvider>();
        var mockFeedIngredientProvider = new Mock<IFeedIngredientProvider>();
        var mockLogger = new Mock<ILogger>();
        _mockDietFactory = new Mock<IDietFactory>();

        _sut = new DefaultDietService(
            mockDietProvider.Object,
            mockFeedIngredientProvider.Object,
            mockLogger.Object,
            _mockDietFactory.Object);
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }

    #endregion

    #region Tests

    [TestMethod]
    public void GetValidAnimalDietTypesReturnsEmptyList()
    {
        var result = _sut.GetValidAnimalDietTypes(AnimalType.Horses);

        Assert.IsFalse(result.Any());
    }

    [TestMethod]
    public void GetDietsReturnsNonEmptyList()
    {
        _mockDietFactory.Setup(factory => factory.GetValidDietKeys()).Returns(
            new List<Tuple<AnimalType, DietType>>()
            {
                new Tuple<AnimalType, DietType>(AnimalType.BeefCow, DietType.LowEnergyAndProtein)
            });

        _mockDietFactory.Setup(factory => factory.Create(It.IsAny<DietType>(), It.IsAny<AnimalType>())).Returns(new DietDto());

        var result = _sut.GetDiets();

        Assert.IsTrue(result.Count > 0);
    }

    [TestMethod]
    public void GetValidAnimalDietTypes_BeefCattle_ReturnsBeefTypes()
    {
        var result = _sut.GetValidAnimalDietTypes(AnimalType.BeefCow);
        CollectionAssert.AreEquivalent(
            new[]
            {
                AnimalType.BeefBackgrounder,
                AnimalType.BeefFinisher,
                AnimalType.BeefCow,
                AnimalType.BeefBulls,
                AnimalType.Stockers
            },
            result.ToArray());
    }

    [TestMethod]
    public void GetValidAnimalDietTypes_DairyCattle_ReturnsDairyTypes()
    {
        var result = _sut.GetValidAnimalDietTypes(AnimalType.DairyLactatingCow);
        CollectionAssert.AreEquivalent(
            new[]
            {
                AnimalType.DairyDryCow,
                AnimalType.DairyHeifers,
                AnimalType.DairyLactatingCow
            },
            result.ToArray());
    }

    [TestMethod]
    public void GetValidAnimalDietTypes_Sheep_ReturnsSheepType()
    {
        var result = _sut.GetValidAnimalDietTypes(AnimalType.Sheep);
        CollectionAssert.AreEquivalent(
            new[]
            {
                AnimalType.Sheep
            },
            result.ToArray());
    }

    [TestMethod]
    public void GetValidAnimalDietTypes_Swine_ReturnsSwineTypes()
    {
        var result = _sut.GetValidAnimalDietTypes(AnimalType.Swine);
        CollectionAssert.AreEquivalent(
            new[]
            {
                AnimalType.Swine,
                AnimalType.SwineBoar,
                AnimalType.SwineDrySow,
                AnimalType.SwineFinisher,
                AnimalType.SwineGrower,
                AnimalType.SwineLactatingSow,
                AnimalType.SwineStarter
            },
            result.ToArray());
    }

    [TestMethod]
    public void GetValidAnimalDietTypes_Other_ReturnsEmpty()
    {
        var result = _sut.GetValidAnimalDietTypes(AnimalType.Horses);
        Assert.AreEqual(0, result.Count);
    }

    #endregion
}
