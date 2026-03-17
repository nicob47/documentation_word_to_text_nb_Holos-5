using H.Core.Factories.Crops;

namespace H.Core.Mappers;

/// <summary>
/// Clones a CropDto. Computed/UI-only properties (GroupedCropItems, SelectedCropTypeItem)
/// are not copied because they are read-only.
/// </summary>
public class CropDtoToCropDtoMapper : IModelMapper<CropDto, CropDto>
{
    public CropDto Map(CropDto source)
        => PropertyMapper.Map<CropDto, CropDto>(source);
}
