using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using H.Core.Enumerations;
using H.Core.Services.CropColorService;

namespace H.Avalonia.Converters;

/// <summary>
/// Converts a CropType enum value to a color brush using the ICropColorService.
/// This converter uses the crop color service to provide consistent coloring across the application.
/// </summary>
public class CropTypeToColorConverter : IValueConverter
{
    private static readonly ICropColorService _cropColorService = new CropColorService();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is CropType cropType)
        {
            try
            {
                var colorHex = _cropColorService.GetCropColorHex(cropType);
                return Brush.Parse(colorHex);
            }
            catch
            {
                // Return default color if parsing fails
                return Brush.Parse("#f2f2f2");
            }
        }

        // Return default light gray for invalid values
        return Brush.Parse("#f2f2f2");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException("CropTypeToColorConverter only supports one-way conversion.");
    }
}
