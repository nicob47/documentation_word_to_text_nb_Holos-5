using H.Core.Enumerations;
using H.Core.Mappers;
using H.Core.Providers.Feed;

namespace H.Core.Test.Mappers;

[TestClass]
public class DietMapperTests
{
    private DietToDietDtoMapper _toDto = null!;
    private DietDtoToDietMapper _fromDto = null!;

    [TestInitialize]
    public void Init()
    {
        _toDto = new DietToDietDtoMapper();
        _fromDto = new DietDtoToDietMapper();
    }

    #region Diet -> DietDto

    [TestMethod]
    public void DietToDto_CopiesScalarProperties()
    {
        var source = new Diet
        {
            Name = "High Energy Beef",
            DietType = DietType.HighEnergyAndProtein,
            AnimalType = AnimalType.BeefCow,
            CrudeProtein = 14.5,
            Forage = 60.0,
            TotalDigestibleNutrient = 68.2,
            DailyDryMatterFeedIntakeOfFeed = 12.3,
            MethaneConversionFactor = 0.07,
            DietaryNetEnergyConcentration = 7.5,
            Ash = 8.1,
            Comments = "Test diet",
            IsDefaultDiet = true,
            IsCustomPlaceholderDiet = false,
        };

        var dto = _toDto.Map(source);

        Assert.AreEqual("High Energy Beef", dto.Name);
        Assert.AreEqual(DietType.HighEnergyAndProtein, dto.DietType);
        Assert.AreEqual(AnimalType.BeefCow, dto.AnimalType);
        Assert.AreEqual(14.5, dto.CrudeProtein);
        Assert.AreEqual(60.0, dto.Forage);
        Assert.AreEqual(68.2, dto.TotalDigestibleNutrient);
        Assert.AreEqual(12.3, dto.DailyDryMatterFeedIntakeOfFeed);
        Assert.AreEqual(0.07, dto.MethaneConversionFactor);
        Assert.AreEqual(7.5, dto.DietaryNetEnergyConcentration);
        Assert.AreEqual(8.1, dto.Ash);
        Assert.AreEqual("Test diet", dto.Comments);
        Assert.IsTrue(dto.IsDefaultDiet);
        Assert.IsFalse(dto.IsCustomPlaceholderDiet);
    }

    [TestMethod]
    public void DietToDto_DeepClonesIngredients()
    {
        var source = new Diet
        {
            Name = "Diet with ingredients",
            AnimalType = AnimalType.BeefCow,
        };
        source.Ingredients.Add(new FeedIngredient { IngredientType = IngredientType.AlfalfaCubes, CrudeProtein = 18.0, PercentageInDiet = 50 });
        source.Ingredients.Add(new FeedIngredient { IngredientType = IngredientType.BarleyGrainRolled, CrudeProtein = 12.5, PercentageInDiet = 50 });

        var dto = _toDto.Map(source);

        Assert.AreEqual(2, dto.Ingredients.Count);
        var dtoIngredients = dto.Ingredients.ToList();
        Assert.AreEqual(IngredientType.AlfalfaCubes, dtoIngredients[0].IngredientType);
        Assert.AreEqual(18.0, dtoIngredients[0].CrudeProtein);
        Assert.AreEqual(50, dtoIngredients[0].PercentageInDiet);
        Assert.AreEqual(IngredientType.BarleyGrainRolled, dtoIngredients[1].IngredientType);

        // Verify they are deep clones, not the same references
        Assert.AreNotSame(source.Ingredients.First(), dtoIngredients[0]);
        Assert.AreNotSame(source.Ingredients.Last(), dtoIngredients[1]);
    }

    [TestMethod]
    public void DietToDto_HandlesEmptyIngredients()
    {
        var source = new Diet { Name = "Empty diet" };

        var dto = _toDto.Map(source);

        Assert.IsNotNull(dto.Ingredients);
        Assert.AreEqual(0, dto.Ingredients.Count);
    }

    [TestMethod]
    public void DietToDto_MutatingDtoIngredientsDoesNotAffectSource()
    {
        var source = new Diet { AnimalType = AnimalType.BeefCow };
        source.Ingredients.Add(new FeedIngredient { CrudeProtein = 10.0 });

        var dto = _toDto.Map(source);
        var dtoIngredient = (FeedIngredient)dto.Ingredients.First();
        dtoIngredient.CrudeProtein = 99.9;

        Assert.AreEqual(10.0, source.Ingredients.First().CrudeProtein);
    }

    #endregion

    #region DietDto -> Diet

    [TestMethod]
    public void DtoToDiet_CopiesNonComputedScalarProperties()
    {
        // Note: Diet.UpdateTotals() recomputes CrudeProtein, Forage, TDN, Ash, etc. from
        // its Ingredients collection whenever ingredients change. So the only scalars that
        // survive a DTO -> Diet copy unchanged are those NOT recomputed by UpdateTotals.
        var dto = new DietDto
        {
            Name = "Avg Quality Forage",
            DietType = DietType.AverageQualityForage,
            AnimalType = AnimalType.Sheep,
            DailyDryMatterFeedIntakeOfFeed = 1.4,
            DietaryNetEnergyConcentration = 6.5,
            Comments = "Round trip",
            IsDefaultDiet = false,
            IsCustomPlaceholderDiet = true,
        };

        var diet = _fromDto.Map(dto);

        Assert.AreEqual("Avg Quality Forage", diet.Name);
        Assert.AreEqual(DietType.AverageQualityForage, diet.DietType);
        Assert.AreEqual(AnimalType.Sheep, diet.AnimalType);
        Assert.AreEqual(1.4, diet.DailyDryMatterFeedIntakeOfFeed);
        Assert.AreEqual(6.5, diet.DietaryNetEnergyConcentration);
        Assert.AreEqual("Round trip", diet.Comments);
        Assert.IsFalse(diet.IsDefaultDiet);
        Assert.IsTrue(diet.IsCustomPlaceholderDiet);
    }

    [TestMethod]
    public void DtoToDiet_RecomputesNutrientsFromIngredients()
    {
        // When the DTO has ingredients, Diet recomputes nutrient totals from them.
        // The DTO's own nutrient values (if any) are overwritten by the recomputation.
        // This mirrors Diet.CopyDiet's documented behaviour.
        var dto = new DietDto { AnimalType = AnimalType.Sheep, DietType = DietType.AverageQualityForage };
        var ingredient = new FeedIngredient
        {
            PercentageInDiet = 100,
            CrudeProtein = 11.0,
            Forage = 97.0,
            TotalDigestibleNutrient = 55.0,
            Ash = 7.0,
        };
        dto.Ingredients = new List<IFeedIngredient> { ingredient };

        var diet = _fromDto.Map(dto);

        // 100% ingredient at these values => Diet's totals recompute to the ingredient values
        Assert.AreEqual(11.0, diet.CrudeProtein);
        Assert.AreEqual(97.0, diet.Forage);
        Assert.AreEqual(55.0, diet.TotalDigestibleNutrient);
        Assert.AreEqual(7.0, diet.Ash);
    }

    [TestMethod]
    public void DtoToDiet_DeepClonesIngredients()
    {
        var dto = new DietDto
        {
            Name = "DTO with ingredients",
            AnimalType = AnimalType.BeefCow,
        };
        var dtoIngredients = new List<IFeedIngredient>
        {
            new FeedIngredient { IngredientType = IngredientType.OatGrain, CrudeProtein = 12.0, PercentageInDiet = 40 },
            new FeedIngredient { IngredientType = IngredientType.SoybeanMealExpelled, CrudeProtein = 44.0, PercentageInDiet = 60 },
        };
        dto.Ingredients = dtoIngredients;

        var diet = _fromDto.Map(dto);

        Assert.AreEqual(2, diet.Ingredients.Count);
        Assert.AreEqual(IngredientType.OatGrain, diet.Ingredients[0].IngredientType);
        Assert.AreEqual(12.0, diet.Ingredients[0].CrudeProtein);
        Assert.AreEqual(40, diet.Ingredients[0].PercentageInDiet);
        Assert.AreEqual(IngredientType.SoybeanMealExpelled, diet.Ingredients[1].IngredientType);

        // Verify they are deep clones, not the same references
        Assert.AreNotSame(dtoIngredients[0], diet.Ingredients[0]);
        Assert.AreNotSame(dtoIngredients[1], diet.Ingredients[1]);
    }

    [TestMethod]
    public void DtoToDiet_HandlesEmptyIngredients()
    {
        var dto = new DietDto { Name = "Empty DTO" };

        var diet = _fromDto.Map(dto);

        Assert.IsNotNull(diet.Ingredients);
        Assert.AreEqual(0, diet.Ingredients.Count);
    }

    #endregion

    #region Round-trip

    [TestMethod]
    public void RoundTrip_PreservesPropertiesWhenIngredientsAreConsistent()
    {
        // To round-trip nutrient values through Diet -> DietDto -> Diet, the source Diet
        // must have ingredients that compute back to those values (since the destination
        // Diet's UpdateTotals() will recompute nutrients from its ingredient list).
        var original = new Diet
        {
            Name = "Round-trip diet",
            DietType = DietType.GoodQualityForage,
            AnimalType = AnimalType.Sheep,
            DailyDryMatterFeedIntakeOfFeed = 2.0,
            Comments = "test",
        };
        original.Ingredients.Add(new FeedIngredient
        {
            PercentageInDiet = 100,
            CrudeProtein = 13.5,
            Forage = 85.0,
            TotalDigestibleNutrient = 60.0,
            Ash = 6.5,
        });

        var dto = _toDto.Map(original);
        var restored = _fromDto.Map(dto);

        Assert.AreEqual(original.Name, restored.Name);
        Assert.AreEqual(original.DietType, restored.DietType);
        Assert.AreEqual(original.AnimalType, restored.AnimalType);
        Assert.AreEqual(original.CrudeProtein, restored.CrudeProtein);
        Assert.AreEqual(original.Forage, restored.Forage);
        Assert.AreEqual(original.TotalDigestibleNutrient, restored.TotalDigestibleNutrient);
        Assert.AreEqual(original.DailyDryMatterFeedIntakeOfFeed, restored.DailyDryMatterFeedIntakeOfFeed);
        Assert.AreEqual(original.Ash, restored.Ash);
        Assert.AreEqual(original.Comments, restored.Comments);
    }

    [TestMethod]
    public void RoundTrip_PreservesIngredientsByValue()
    {
        var original = new Diet { AnimalType = AnimalType.BeefCow };
        original.Ingredients.Add(new FeedIngredient { IngredientType = IngredientType.AlfalfaCubes, CrudeProtein = 18.0, PercentageInDiet = 100 });

        var dto = _toDto.Map(original);
        var restored = _fromDto.Map(dto);

        Assert.AreEqual(1, restored.Ingredients.Count);
        Assert.AreEqual(IngredientType.AlfalfaCubes, restored.Ingredients[0].IngredientType);
        Assert.AreEqual(18.0, restored.Ingredients[0].CrudeProtein);
        Assert.AreEqual(100, restored.Ingredients[0].PercentageInDiet);
        Assert.AreNotSame(original.Ingredients[0], restored.Ingredients[0]);
    }

    #endregion
}
