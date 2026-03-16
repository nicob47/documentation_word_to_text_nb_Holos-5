using H.Core.Enumerations;
using H.Core.Mappers;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
using H.Core.Services.Initialization;
using Prism.Ioc;

namespace H.Core.Factories.Crops;

/// <summary>
/// A class used to create new <see cref="CropDto"/> and <see cref="CropViewItem"/> instances. The class will provide basic initialization of a new instance before returning the result to the caller.
/// </summary>
public class CropFactory : ICropFactory
{
    #region Fields

    private readonly IModelMapper<CropViewItem, CropDto> _cropViewItemToDtoMapper;
    private readonly IModelMapper<CropDto, CropDto> _cropDtoToDtoMapper;
    private readonly IModelMapper<ICropDto, CropViewItem> _cropDtoToViewItemMapper;

    private readonly ICropInitializationService _cropInitializationService;

    #endregion

    #region Constructors

    public CropFactory(ICropInitializationService cropInitializationService, IContainerProvider containerProvider)
    {
        if (cropInitializationService != null)
        {
            _cropInitializationService = cropInitializationService; 
        }
        else
        {
            throw new ArgumentNullException(nameof(cropInitializationService));
        }

        if (containerProvider == null)
        {
            throw new ArgumentNullException(nameof(containerProvider));
        }
        else
        {
            _cropViewItemToDtoMapper = containerProvider.Resolve<IModelMapper<CropViewItem, CropDto>>(nameof(CropViewItemToCropDtoMapper));
            _cropDtoToDtoMapper = containerProvider.Resolve<IModelMapper<CropDto, CropDto>>(nameof(CropDtoToCropDtoMapper));
            _cropDtoToViewItemMapper = containerProvider.Resolve<IModelMapper<ICropDto, CropViewItem>>(nameof(CropDtoToCropViewItemMapper));
        }
    }

    #endregion

    #region Public Methods

    public CropDto CreateDto()
    {
        return new CropDto();
    }

    public CropDto CreateDto(Farm farm)
    {
        var cropViewItem = new CropViewItem();

        cropViewItem.CropType = CropType.Wheat;
        cropViewItem.Year = DateTime.Now.Year;
        cropViewItem.Name = $"{cropViewItem.CropType} {cropViewItem.Year}";

        _cropInitializationService.Initialize(cropViewItem, farm);

        var dto = this.CreateCropDto(cropViewItem);

        return dto;
    }

    public IDto CreateDtoFromDtoTemplate(IDto template)
    {
        if (template is CropDto dtoTemplate)
        {
            return _cropDtoToDtoMapper.Map(dtoTemplate);
        }

        return new CropDto();
    }

    public CropDto CreateCropDto(CropViewItem template)
    {
        return _cropViewItemToDtoMapper.Map(template);
    }

    public CropViewItem CreateCropViewItem(ICropDto cropDto)
    {
        return _cropDtoToViewItemMapper.Map(cropDto);
    }

    #endregion
}