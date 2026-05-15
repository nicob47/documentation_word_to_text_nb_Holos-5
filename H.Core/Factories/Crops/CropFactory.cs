using H.Core.Enumerations;
using H.Core.Mappers;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
using H.Core.Services.Initialization;
using Prism.Ioc;

namespace H.Core.Factories.Crops;

/// <summary>
/// Default <see cref="ICropFactory"/> implementation. Holds three DI-resolved
/// <see cref="IModelMapper{TSource, TDest}"/> instances that handle the actual property
/// copies between CropViewItem ↔ CropDto + DTO ↔ DTO clones, and an
/// <see cref="ICropInitializationService"/> reference that seeds new crops with sensible
/// per-province / per-crop defaults (lignin content, biomass coefficients, etc.) before they
/// reach the user.
///
/// <para><b>Three mapper roles:</b></para>
/// <list type="bullet">
///   <item><c>CropViewItemToCropDtoMapper</c> — domain → DTO when opening the editor for an existing crop.</item>
///   <item><c>CropDtoToCropViewItemMapper</c> — DTO → domain on save.</item>
///   <item><c>CropDtoToCropDtoMapper</c> — clone-a-DTO, used when the editor needs to keep an isolated copy (e.g. "Save As" workflows).</item>
/// </list>
/// All three are resolved by named key out of the container in the constructor.
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

    public CropDto CreateCoverCropDto(int year)
    {
        var coverCropTypes = CropTypeExtensions.GetValidCoverCropTypes().ToList();
        var defaultType = coverCropTypes.FirstOrDefault(ct => ct != CropType.None);

        var dto = new CropDto
        {
            IsSecondaryCrop = true,
            Year = year,
            CropType = defaultType,
            CoverCropTerminationType = CoverCropTerminationType.Natural,
        };

        return dto;
    }

    #endregion
}