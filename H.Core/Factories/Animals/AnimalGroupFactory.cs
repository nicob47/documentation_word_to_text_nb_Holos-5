using H.Core.Mappers;
using H.Core.Models;
using Prism.Ioc;

namespace H.Core.Factories.Animals;

/// <summary>
/// Factory for creating <see cref="AnimalGroupDto"/> instances with optional mapper support.
/// </summary>
public class AnimalGroupFactory : IAnimalGroupFactory
{
    #region Fields

    /// <summary>
    /// Optional mapper for copying data between <see cref="AnimalGroupDto"/> instances.
    /// Will be null if the factory is created without dependency injection.
    /// </summary>
    private readonly IModelMapper<AnimalGroupDto, AnimalGroupDto>? _animalGroupDtoToAnimalGroupDtoMapper;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimalGroupFactory"/> class without dependency injection.
    /// The mapper will be unavailable, so <see cref="CreateDtoFromDtoTemplate"/> will skip mapping operations.
    /// </summary>
    public AnimalGroupFactory()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimalGroupFactory"/> class with dependency injection.
    /// </summary>
    /// <param name="containerProvider">The container provider used to resolve the mapper instance.</param>
    public AnimalGroupFactory(IContainerProvider containerProvider)
    {
        _animalGroupDtoToAnimalGroupDtoMapper = containerProvider.Resolve<IModelMapper<AnimalGroupDto, AnimalGroupDto>>(nameof(AnimalGroupDtoToAnimalGroupDtoMapper));
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Creates a new <see cref="AnimalGroupDto"/> instance with default values.
    /// </summary>
    /// <returns>A new <see cref="AnimalGroupDto"/> instance.</returns>
    public AnimalGroupDto CreateDto()
    {
        return new AnimalGroupDto();
    }

    /// <summary>
    /// Creates a new <see cref="AnimalGroupDto"/> instance for the specified farm.
    /// </summary>
    /// <param name="farm">The farm context (currently unused).</param>
    /// <returns>A new <see cref="AnimalGroupDto"/> instance.</returns>
    public AnimalGroupDto CreateDto(Farm farm)
    {
        return new AnimalGroupDto();
    }

    /// <summary>
    /// Creates a new <see cref="AnimalGroupDto"/> instance by copying data from a template.
    /// If a mapper is available, properties will be copied from the template; otherwise, default values are used.
    /// </summary>
    /// <param name="template">The template DTO to copy data from.</param>
    /// <returns>A new <see cref="AnimalGroupDto"/> instance with data copied from the template if mapping is available.</returns>
    public IDto CreateDtoFromDtoTemplate(IDto template)
    {
        var result = new AnimalGroupDto();

        if (_animalGroupDtoToAnimalGroupDtoMapper != null && template is AnimalGroupDto dtoTemplate)
        {
            PropertyMapper.CopyTo(dtoTemplate, result);
        }

        return result;
    }

    #endregion
}