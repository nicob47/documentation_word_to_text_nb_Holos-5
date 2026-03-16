using H.Core.Mappers;
using H.Core.Models;
using Prism.Ioc;

namespace H.Core.Factories.Animals;

public class AnimalComponentFactory : IAnimalComponentFactory
{
    #region Fields

    private readonly IModelMapper<AnimalComponentDto, AnimalComponentDto> _animalComponentDtoToAnimalComponentDtoMapper;

    #endregion

    #region Constructors

    public AnimalComponentFactory(IContainerProvider containerProvider)
    {
        if (containerProvider != null)
        {
            _animalComponentDtoToAnimalComponentDtoMapper = containerProvider.Resolve<IModelMapper<AnimalComponentDto, AnimalComponentDto>>(nameof(AnimalComponentDtoToAnimalComponentDtoMapper));
        }
        else
        {
            throw new ArgumentNullException(nameof(containerProvider));
        }
    }

    #endregion

    #region Public Methods

    public AnimalComponentDto CreateDto()
    {
        return new AnimalComponentDto();
    }

    public AnimalComponentDto CreateDto(Farm farm)
    {
        return new AnimalComponentDto();
    }

    public IDto CreateDtoFromDtoTemplate(IDto template)
    {
        var result = new AnimalComponentDto();

        if (template is AnimalComponentDto dtoTemplate)
        {
            PropertyMapper.CopyTo(dtoTemplate, result);
        }

        return result;
    }

    #endregion
}