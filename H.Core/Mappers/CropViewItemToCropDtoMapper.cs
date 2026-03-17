using H.Core.Factories.Crops;
using H.Core.Models.LandManagement.Fields;

namespace H.Core.Mappers;

/// <summary>
/// Maps CropViewItem (v4 domain model) to CropDto (v5 DTO).
/// CropViewItem.Yield maps to CropDto.WetYield.
/// </summary>
public class CropViewItemToCropDtoMapper : IModelMapper<CropViewItem, CropDto>
{
    public CropDto Map(CropViewItem source)
    {
        var dest = PropertyMapper.Map<CropViewItem, CropDto>(source);
        dest.WetYield = source.Yield;
        return dest;
    }
}
