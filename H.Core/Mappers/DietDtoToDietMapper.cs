using H.Core.Providers.Feed;

namespace H.Core.Mappers;

/// <summary>
/// Maps a UI-side <see cref="DietDto"/> back to a domain <see cref="Diet"/>.
/// Scalar properties are copied by convention via <see cref="PropertyMapper"/>; the
/// <see cref="DietDto.Ingredients"/> collection is deep-cloned manually since
/// <see cref="PropertyMapper"/> skips collection properties to avoid reference sharing.
/// Important: we re-use the destination Diet's existing Ingredients collection (created
/// by the Diet constructor) instead of replacing it. The constructor wires a
/// CollectionChanged handler to that specific collection instance; replacing the
/// collection orphans the handler so adding ingredients no longer triggers
/// <see cref="Diet.UpdateTotals"/>, leaving nutrient scalars at zero.
/// </summary>
public class DietDtoToDietMapper : IModelMapper<DietDto, Diet>
{
    public Diet Map(DietDto source)
    {
        var dest = PropertyMapper.Map<DietDto, Diet>(source);

        // Clear + Add against the constructor-wired Ingredients collection so that
        // each Add fires CollectionChanged -> UpdateTotals, recomputing nutrient
        // scalars from the new ingredient list. This makes the resulting Diet
        // self-consistent regardless of what scalar values the DTO carried.
        dest.Ingredients.Clear();
        if (source.Ingredients != null)
        {
            foreach (var ingredient in source.Ingredients)
            {
                if (ingredient is FeedIngredient concrete)
                {
                    dest.Ingredients.Add(PropertyMapper.Map<FeedIngredient, FeedIngredient>(concrete));
                }
            }
        }

        return dest;
    }
}
