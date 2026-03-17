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
        return dest;
    }
}
