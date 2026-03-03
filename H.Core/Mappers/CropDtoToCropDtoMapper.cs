using AutoMapper;
using H.Core.Factories.Crops;

namespace H.Core.Mappers;

public class CropDtoToCropDtoMapper : Profile
{
    public CropDtoToCropDtoMapper()
    {
        CreateMap<CropDto, CropDto>()
            .ForMember(destinationMember: dto => dto.GroupedCropItems, memberOptions: options => options.Ignore())
            .ForMember(destinationMember: dto => dto.SelectedCropTypeItem, memberOptions: options => options.Ignore());
    }
}