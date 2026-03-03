using H.Core.Enumerations;

namespace H.Core.Providers.Feed
{
    public interface IFeedIngredientProvider
    {
        IList<IFeedIngredient>? GetBeefFeedIngredients();
        IList<IFeedIngredient>? GetDairyFeedIngredients();
        IList<IFeedIngredient>? GetSwineFeedIngredients();
        IReadOnlyCollection<IFeedIngredient> GetIngredientsForDiet(AnimalType animalType, DietType dietType);
        IReadOnlyCollection<IFeedIngredient> GetAllIngredientsForAnimalType(AnimalType animalType);

        FeedIngredient? CopyIngredient(IFeedIngredient ingredient, double defaultPercentageInDiet);
    }
}