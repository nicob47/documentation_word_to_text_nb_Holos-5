using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace H.Avalonia.Converters;

/// <summary>
/// Converts a boolean selection state to a border thickness for highlighting selected cells.
/// When true (selected), returns a thicker border. When false, returns a normal border.
/// </summary>
public class BoolToSelectionBorderThicknessConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected && isSelected)
        {
            // Selected: thicker border (3px all sides)
            return new Thickness(3);
        }

        // Not selected: normal border (1px all sides)
        return new Thickness(1);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException("BoolToSelectionBorderThicknessConverter only supports one-way conversion.");
    }
}

/// <summary>
/// Converts a boolean selection state to a border brush color for highlighting selected cells.
/// When true (selected), returns a highlight color. When false, returns the normal border color.
/// </summary>
public class BoolToSelectionBorderBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected && isSelected)
        {
            // Selected: blue highlight border
            return Brush.Parse("#1f497a");
        }

        // Not selected: normal gray border
        return Brush.Parse("#d9d9d9");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException("BoolToSelectionBorderBrushConverter only supports one-way conversion.");
    }
}

