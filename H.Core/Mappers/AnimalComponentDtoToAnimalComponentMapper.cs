using H.Core.Factories.Animals;
using H.Core.Models.Animals;

namespace H.Core.Mappers;

public class AnimalComponentDtoToAnimalComponentMapper : IModelMapper<AnimalComponentDto, AnimalComponentBase>
{
    /// <summary>
    /// This mapper cannot create new instances because AnimalComponentBase is abstract.
    /// Use PropertyMapper.CopyTo(source, dest) with an existing concrete instance instead.
    /// </summary>
    public AnimalComponentBase Map(AnimalComponentDto source)
        => throw new NotSupportedException(
            $"Cannot create a new instance of abstract type {nameof(AnimalComponentBase)}. " +
            $"Use {nameof(PropertyMapper)}.CopyTo(source, dest) with a concrete instance instead.");
}
