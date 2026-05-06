using H.Core.Providers.Feed;

namespace H.Core.Mappers;

/// <summary>
/// Maps a domain <see cref="Diet"/> to a <see cref="DietDto"/> for UI binding.
/// Scalar properties are copied by convention via <see cref="PropertyMapper"/>; the
/// <see cref="Diet.Ingredients"/> collection is deep-cloned manually since
/// <see cref="PropertyMapper"/> skips collection properties to avoid reference sharing.
/// </summary>
public class DietToDietDtoMapper : IModelMapper<Diet, DietDto>
{
    public DietDto Map(Diet source)
    {
        var dest = PropertyMapper.Map<Diet, DietDto>(source);

        // Manually deep-clone the Ingredients collection so the DTO owns its own copies.
        var clonedIngredients = new List<IFeedIngredient>();
        if (source.Ingredients != null)
        {
            foreach (var ingredient in source.Ingredients)
            {
                clonedIngredients.Add(PropertyMapper.Map<FeedIngredient, FeedIngredient>(ingredient));
            }
        }
        dest.Ingredients = clonedIngredients;

        return dest;
    }
}
