using H.Core.Mappers;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
using Prism.Ioc;

namespace H.Core.Factories.Fields;

/// <summary>
/// A class used to create new <see cref="FieldSystemComponentDto"/> instances. The class will provide basic initialization of a new instance before returning the result to the caller.
/// </summary>
public class FieldFactory : IFieldFactory
{
    #region Fields

    private readonly IModelMapper<FieldSystemComponent, FieldSystemComponentDto> _fieldComponentToDtoMapper;
    private readonly IModelMapper<FieldSystemComponentDto, FieldSystemComponentDto> _fieldDtoToDtoMapper;

    #endregion

    #region Constructors

    public FieldFactory(IContainerProvider containerProvider)
    {
        if (containerProvider == null)
        {
            throw new ArgumentNullException(nameof(containerProvider));
        }

        _fieldComponentToDtoMapper = containerProvider.Resolve<IModelMapper<FieldSystemComponent, FieldSystemComponentDto>>(nameof(FieldComponentToDtoMapper));
        _fieldDtoToDtoMapper = containerProvider.Resolve<IModelMapper<FieldSystemComponentDto, FieldSystemComponentDto>>(nameof(FieldDtoToFieldDtoMapper));
    }

    #endregion

    #region Public Methods

    public IDto CreateDtoFromDtoTemplate(IDto template)
    {
        if (template is FieldSystemComponentDto dtoTemplate)
        {
            return _fieldDtoToDtoMapper.Map(dtoTemplate);
        }

        return new FieldSystemComponentDto();
    }

    /// <summary>
    /// Create a new instance with no additional configuration to a default instance.
    /// </summary>
    public FieldSystemComponentDto Create()
    {
        return new FieldSystemComponentDto();
    }

    public FieldSystemComponentDto CreateDto()
    {
        return new FieldSystemComponentDto();
    }

    public FieldSystemComponentDto CreateDto(Farm farm)
    {
        return new FieldSystemComponentDto();
    }

    public FieldSystemComponentDto CreateFieldDto(IFieldComponentDto template)
    {
        var fieldComponentDto = new FieldSystemComponentDto();

        PropertyMapper.CopyTo(template, fieldComponentDto);

        return fieldComponentDto;
    }

    #endregion

    #region Private Methods

    #endregion
}