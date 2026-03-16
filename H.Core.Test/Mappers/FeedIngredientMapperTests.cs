using H.Core.Mappers;
using H.Core.Providers.Feed;

namespace H.Core.Test.Mappers;

[TestClass]
public class FeedIngredientMapperTests
{
    [TestMethod]
    public void FeedIngredientToFeedIngredient_ClonesAllProperties()
    {
        var mapper = new FeedIngredientToFeedIngredientMapper();
        var source = new FeedIngredient
        {
            IngredientType = Enumerations.IngredientType.AlfalfaCubes,
            CrudeProtein = 18.5,
            Starch = 30.0,
            Fat = 4.2,
            Ash = 6.1
        };

        var result = mapper.Map(source);

        Assert.AreNotSame(source, result);
        Assert.AreEqual(Enumerations.IngredientType.AlfalfaCubes, result.IngredientType);
        Assert.AreEqual(18.5, result.CrudeProtein);
        Assert.AreEqual(30.0, result.Starch);
        Assert.AreEqual(4.2, result.Fat);
        Assert.AreEqual(6.1, result.Ash);
    }
}
