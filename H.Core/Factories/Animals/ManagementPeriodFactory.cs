using H.Core.Mappers;
using H.Core.Models.Animals;
using Prism.Ioc;

namespace H.Core.Factories.Animals;

public class ManagementPeriodFactory : IManagementPeriodFactory
{
    #region Fields

    private readonly IModelMapper<ManagementPeriodDto, ManagementPeriodDto> _managementPeriodDtoToManagementPeriodDtoMapper;
    private readonly IModelMapper<ManagementPeriod, ManagementPeriodDto> _managementPeriodToManagementPeriodDtoMapper;
    private readonly IModelMapper<ManagementPeriodDto, ManagementPeriod> _managementPeriodDtoToManagementPeriodMapper;

    #endregion

    #region Constructors

    public ManagementPeriodFactory(IContainerProvider containerProvider)
    {
        if (containerProvider == null)
        {
            throw new ArgumentNullException(nameof(containerProvider));
        }
        else
        {
            _managementPeriodDtoToManagementPeriodDtoMapper = containerProvider.Resolve<IModelMapper<ManagementPeriodDto, ManagementPeriodDto>>(nameof(ManagementPeriodDtoToManagementPeriodDtoMapper));
            _managementPeriodToManagementPeriodDtoMapper = containerProvider.Resolve<IModelMapper<ManagementPeriod, ManagementPeriodDto>>(nameof(ManagementPeriodToManagementPeriodDtoMapper));
            _managementPeriodDtoToManagementPeriodMapper = containerProvider.Resolve<IModelMapper<ManagementPeriodDto, ManagementPeriod>>(nameof(ManagementPeriodDtoToManagementPeriodMapper));
        }
    }

    #endregion

    #region Public Methods

    public IManagementPeriodDto CreateManagementPeriodDto()
    {
        var dto = new ManagementPeriodDto();
        dto.Name = "New Management Period";
        dto.Start = new DateTime(DateTime.Now.Year, 1, 1);
        dto.End = new DateTime(DateTime.Now.Year, 12, 31);
        dto.NumberOfDays = (dto.End - dto.Start).Days + 1;

        return dto;
    }

    public IManagementPeriodDto CreateDtoFromDtoTemplate(IManagementPeriodDto template)
    {
        var dto = new ManagementPeriodDto();

        if (template is ManagementPeriodDto dtoTemplate)
        {
            PropertyMapper.CopyTo(dtoTemplate, dto);
        }

        return dto;
    }

    public IManagementPeriodDto CreateManagementPeriodDto(ManagementPeriod managementPeriod)
    {
        return _managementPeriodToManagementPeriodDtoMapper.Map(managementPeriod);
    }

    public ManagementPeriod CreateManagementPeriod(IManagementPeriodDto dto)
    {
        if (dto is ManagementPeriodDto managementPeriodDto)
        {
            return _managementPeriodDtoToManagementPeriodMapper.Map(managementPeriodDto);
        }

        return new ManagementPeriod();
    }

    #endregion
}