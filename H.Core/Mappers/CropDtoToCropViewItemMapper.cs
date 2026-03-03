using AutoMapper;
using H.Core.Factories.Crops;
using H.Core.Models.LandManagement.Fields;

namespace H.Core.Mappers;

/// <summary>
/// AutoMapper profile for mapping a <see cref="ICropDto"/> (the v5 data-transfer object) back
/// to a <see cref="CropViewItem"/> (the v4 domain model).
///
/// Property notes:
/// <list type="bullet">
///   <item>
///     <see cref="CropViewItem.Yield"/> is mapped from <see cref="ICropDto.WetYield"/> —
///     the inverse of the <see cref="CropViewItemToCropDtoMapper"/> mapping.
///   </item>
/// </list>
///
/// No ignores are required here because <see cref="CropViewItem"/> does not contain the
/// computed UI-only properties (<c>GroupedCropItems</c>, <c>SelectedCropTypeItem</c>) that
/// exist on <see cref="CropDto"/>.
/// </summary>
public class CropDtoToCropViewItemMapper : Profile
{
    public CropDtoToCropViewItemMapper()
    {
        CreateMap<ICropDto, CropViewItem>()
            .ForMember(destinationMember: cropViewItem => cropViewItem.Yield, memberOptions: options => options.MapFrom(cropDto => cropDto.WetYield));
    }
}
