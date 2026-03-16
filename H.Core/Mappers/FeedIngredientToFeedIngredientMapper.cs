using H.Core.Providers.Feed;

namespace H.Core.Mappers;

public class FeedIngredientToFeedIngredientMapper : IModelMapper<FeedIngredient, FeedIngredient>
{
    public FeedIngredient Map(FeedIngredient source)
        => PropertyMapper.Map<FeedIngredient, FeedIngredient>(source);
}
