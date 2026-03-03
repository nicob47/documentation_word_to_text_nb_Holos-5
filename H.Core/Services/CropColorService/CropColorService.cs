using H.Core.Enumerations;

namespace H.Core.Services.CropColorService;

/// <summary>
/// Default implementation of <see cref="ICropColorService"/>.
/// Provides consistent color coding and display names for crop types across the application.
/// Uses the CropTypeExtensions class to determine crop categories, with explicit fallbacks
/// for crop types that do not match those extension methods.
/// </summary>
public class CropColorService : ICropColorService
{
    #region Public Methods

    /// <summary>
    /// Gets the hexadecimal color code for a given crop type based on its category.
    /// </summary>
    /// <param name="cropType">The crop type to get the color for</param>
    /// <returns>Hexadecimal color string (e.g., "#FFF3E0")</returns>
    public string GetCropColorHex(CropType cropType)
    {
        // Fallow - Gray
        if (cropType.IsFallow())
            return "#FAFAFA";

        // Cereals/Small Grains - Orange
        if (cropType.IsSmallGrains())
            return "#FFF3E0";

        // Oilseeds - Green
        if (cropType.IsOilSeed())
            return "#E8F5E9";

        // Pulses - Blue
        if (cropType.IsPulseCrop())
            return "#E3F2FD";

        // Forages/Perennials - Purple
        if (cropType.IsPerennial())
            return "#F3E5F5";

        // Root crops - Light Brown
        if (cropType.IsRootCrop())
            return "#EFEBE9";

        // Silage crops - Light Yellow
        if (cropType.IsSilageCrop())
            return "#FFFDE7";

        // Explicit fallbacks for crop types not covered by the extension methods above
        return cropType switch
        {
            // Cereals
            CropType.Rye or CropType.Corn or CropType.Durum
                => "#FFF3E0",

            // Oilseeds
            CropType.FlaxSeed or CropType.Sunflower or CropType.SunflowerSeed or CropType.MustardSeed
                => "#E8F5E9",

            // Pulses
            CropType.Peas or CropType.Beans or CropType.DryBean or CropType.FabaBeans
                => "#E3F2FD",

            // Forages
            CropType.AlfalfaMedicagoSativaL or CropType.AlfalfaHay or CropType.GrassHay
                => "#F3E5F5",

            // Default - Light gray
            _ => "#F5F5F5",
        };
    }

    /// <summary>
    /// Gets the display name for a crop type.
    /// </summary>
    /// <param name="cropType">The crop type to get the display name for</param>
    /// <returns>Display name (e.g., "Wheat", "Canola")</returns>
    public string GetCropDisplayName(CropType cropType)
    {
        return cropType switch
        {
            // Cereals
            CropType.Wheat => "Wheat",
            CropType.Barley => "Barley",
            CropType.Oats => "Oats",
            CropType.Rye => "Rye",
            CropType.Triticale => "Triticale",
            CropType.Durum => "Durum",
            CropType.SpringWheat => "Spring Wheat",
            CropType.WinterWheat => "Winter Wheat",
            CropType.Corn => "Corn",
            CropType.GrainCorn => "Grain Corn",
            CropType.SilageCorn => "Silage Corn",

            // Oilseeds
            CropType.Canola => "Canola",
            CropType.Flax => "Flax",
            CropType.FlaxSeed => "Flax Seed",
            CropType.Sunflower => "Sunflower",
            CropType.SunflowerSeed => "Sunflower Seed",
            CropType.Soybeans => "Soybeans",
            CropType.Mustard => "Mustard",
            CropType.MustardSeed => "Mustard Seed",

            // Pulses
            CropType.Peas => "Peas",
            CropType.DryPeas => "Dry Peas",
            CropType.FieldPeas => "Field Peas",
            CropType.Lentils => "Lentils",
            CropType.Beans => "Beans",
            CropType.DryBean => "Dry Bean",
            CropType.Chickpeas => "Chickpeas",
            CropType.FabaBeans => "Faba Beans",
            CropType.ColouredWhiteFabaBeans => "Coloured White Faba Beans",

            // Forages
            CropType.AlfalfaMedicagoSativaL => "Alfalfa",
            CropType.AlfalfaHay => "Alfalfa Hay",
            CropType.TameLegume => "Tame Legume",
            CropType.TameGrass => "Tame Grass",
            CropType.TameMixed => "Tame Mixed",
            CropType.Forage => "Forage",
            CropType.GrassHay => "Grass Hay",
            CropType.PerennialForages => "Perennial Forages",

            // Fallow
            CropType.Fallow => "Fallow",
            CropType.SummerFallow => "Summer Fallow",

            // Default fallback
            _ => cropType.ToString()
        };
    }

    #endregion
}
