using H.Core.Factories.Crops;
using H.Core.Models.LandManagement.Fields;

namespace H.Core.Mappers;

/// <summary>
/// Maps ICropDto (v5 DTO) back to CropViewItem (v4 domain model).
/// WetYield on the DTO maps to Yield on CropViewItem.
/// </summary>
public class CropDtoToCropViewItemMapper : IModelMapper<ICropDto, CropViewItem>
{
    public CropViewItem Map(ICropDto source)
    {
        var dest = new CropViewItem();
        PropertyMapper.CopyTo(source, dest);
        dest.Yield = source.WetYield;

        // PropertyMapper skips the Guid property by design, but when a DTO is being
        // added to the model as a new CropViewItem, the view item must share the DTO's
        // Guid so that subsequent lookups via FieldComponentService.GetCropViewItemFromDto
        // / RemoveCropFromSystem (which match by Guid) can find the correct view item
        // when the user later edits or removes that crop.
        dest.Guid = source.Guid;
        return dest;
    }
}
