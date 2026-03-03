using AutoMapper;
using H.Core.Factories.Crops;

namespace H.Core.Mappers;

/// <summary>
/// AutoMapper profile for mapping a <see cref="CropDto"/> to another <see cref="CropDto"/>.
/// Used when cloning or copying a crop DTO (e.g. when creating per-cell copies in the
/// rotation preview grid so each cell has its own independent data).
///
/// Computed / UI-only properties that cannot be written by AutoMapper are explicitly
/// ignored to prevent <see cref="System.NotSupportedException"/> at runtime:
/// <list type="bullet">
///   <item><see cref="CropDto.GroupedCropItems"/> — read-only <c>IReadOnlyList&lt;object&gt;</c> built lazily from <see cref="CropDto.ValidCropTypes"/>.</item>
///   <item><see cref="CropDto.SelectedCropTypeItem"/> — write-through alias for <see cref="CropDto.CropType"/>; not a real backing field.</item>
/// </list>
/// </summary>
public class CropDtoToCropDtoMapper : Profile
{
    public CropDtoToCropDtoMapper()
    {
        CreateMap<CropDto, CropDto>()
            .ForMember(destinationMember: dto => dto.GroupedCropItems, memberOptions: options => options.Ignore())
            .ForMember(destinationMember: dto => dto.SelectedCropTypeItem, memberOptions: options => options.Ignore());
    }
}
