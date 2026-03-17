using H.Core.Factories.Animals;

namespace H.Core.Mappers;

public class ManagementPeriodDtoToManagementPeriodDtoMapper : IModelMapper<ManagementPeriodDto, ManagementPeriodDto>
{
    public ManagementPeriodDto Map(ManagementPeriodDto source)
        => PropertyMapper.Map<ManagementPeriodDto, ManagementPeriodDto>(source);
}
