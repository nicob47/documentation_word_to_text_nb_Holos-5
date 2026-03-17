using H.Core.Mappers;
using H.Core.Models;
using H.Core.Models.Climate;
using H.Core.Providers.Climate;
using Prism.Ioc;

namespace H.Core.Factories.Climate;

/// <summary>
/// A class used to create new <see cref="DailyClimateDto"/> and <see cref="DailyClimateData"/> instances. The class will provide basic initialization of a new instance before returning the result to the caller.
/// </summary>
public class DailyClimateDataFactory : IDailyClimateDataFactory
{
    #region Fields

    private readonly IModelMapper<DailyClimateData, DailyClimateDto> _dailyClimateDataToDtoMapper;
    private readonly IModelMapper<DailyClimateDto, DailyClimateDto> _dailyClimateDtoToDtoMapper;
    private readonly IModelMapper<DailyClimateDto, DailyClimateData> _dailyClimateDtoToDataMapper;

    #endregion

    #region Constructors

    public DailyClimateDataFactory(IContainerProvider containerProvider)
    {
        if (containerProvider != null)
        {
            _dailyClimateDataToDtoMapper = containerProvider.Resolve<IModelMapper<DailyClimateData, DailyClimateDto>>(nameof(DailyClimateDataToDailyClimateDtoMapper));
            _dailyClimateDtoToDtoMapper = containerProvider.Resolve<IModelMapper<DailyClimateDto, DailyClimateDto>>(nameof(DailyClimateDtoToDailyClimateDtoMapper));
            _dailyClimateDtoToDataMapper = containerProvider.Resolve<IModelMapper<DailyClimateDto, DailyClimateData>>(nameof(DailyClimateDtoToDailyClimateDataMapper));
        }
        else
        {
            throw new ArgumentNullException(nameof(containerProvider));
        }
    }

    #endregion

    #region Public Methods

    public DailyClimateDto CreateDto()
    {
        return new DailyClimateDto();
    }

    public DailyClimateDto CreateDto(Farm farm)
    {
        // Create a basic DTO with default climate data for the farm
        var dto = new DailyClimateDto
        {
            Year = DateTime.Now.Year
        };

        return dto;
    }

    public IDto CreateDtoFromDtoTemplate(IDto template)
    {
        var dailyClimateDto = new DailyClimateDto();

        if (template is DailyClimateDto dtoTemplate)
        {
            PropertyMapper.CopyTo(dtoTemplate, dailyClimateDto);
        }

        return dailyClimateDto;
    }

    public DailyClimateDto CreateDto(DailyClimateData dailyClimateData)
    {
        return _dailyClimateDataToDtoMapper.Map(dailyClimateData);
    }

    public DailyClimateData CreateData(DailyClimateDto dailyClimateDto)
    {
        return _dailyClimateDtoToDataMapper.Map(dailyClimateDto);
    }

    #endregion
}