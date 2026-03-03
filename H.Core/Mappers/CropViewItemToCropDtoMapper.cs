using AutoMapper;
using H.Core.Factories.Crops;
using H.Core.Models.LandManagement.Fields;

namespace H.Core.Mappers;

/// <summary>
/// AutoMapper profile for mapping a <see cref="CropViewItem"/> (the v4 domain model) to a
/// <see cref="CropDto"/> (the v5 data-transfer object used by the Avalonia UI).
///
/// Property notes:
/// <list type="bullet">
///   <item>
///     <see cref="CropDto.WetYield"/> is mapped from <see cref="CropViewItem.Yield"/> because
///     the v4 model stores a single <c>Yield</c> field that corresponds to wet yield in v5.
///   </item>
///   <item>
///     <see cref="CropDto.GroupedCropItems"/> is ignored — it is a read-only
///     <c>IReadOnlyList&lt;object&gt;</c> built lazily from <see cref="CropDto.ValidCropTypes"/>
///     and cannot be set by AutoMapper (would throw <see cref="System.NotSupportedException"/>).
///   </item>
///   <item>
///     <see cref="CropDto.SelectedCropTypeItem"/> is ignored — it is a write-through alias
///     for <see cref="CropDto.CropType"/> used by the grouped ComboBox and has no backing field.
///   </item>
/// </list>
/// </summary>
public class CropViewItemToCropDtoMapper : Profile
{
    public CropViewItemToCropDtoMapper()
    {
        CreateMap<CropViewItem, CropDto>()
            .ForMember(destinationMember: dto => dto.WetYield, memberOptions: options => options.MapFrom(cropViewItem => cropViewItem.Yield))
            .ForMember(destinationMember: dto => dto.GroupedCropItems, memberOptions: options => options.Ignore())
            .ForMember(destinationMember: dto => dto.SelectedCropTypeItem, memberOptions: options => options.Ignore());
    }
}
