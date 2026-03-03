using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace H.Avalonia.Converters;

/// <summary>
/// Converts a boolean value to a background brush for selection states
/// </summary>
public class BoolToBackgroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected ? Brush.Parse("#ebf2fa") : Brush.Parse("White");
        }
        
        return Brush.Parse("White");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a boolean value to a border brush for selection states
/// </summary>
public class BoolToBorderBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected ? Brush.Parse("#1f497a") : Brush.Parse("#8c8c8c");
        }
        
        return Brush.Parse("#8c8c8c");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a boolean value to a border thickness for selection states
/// </summary>
public class BoolToBorderThicknessConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected ? new global::Avalonia.Thickness(3) : new global::Avalonia.Thickness(2);
        }
        
        return new global::Avalonia.Thickness(2);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Inverts a boolean value (true -> false, false -> true)
/// </summary>
public class InverseBooleanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        
        return true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        
        return false;
    }
}

/// <summary>
/// Converts a boolean value to a render transform for the "pressed in" effect
/// </summary>
public class BoolToScaleTransformConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            var scaleValue = isSelected ? 0.98 : 1.0;
            return new ScaleTransform(scaleValue, scaleValue);
        }
        
        return new ScaleTransform(1.0, 1.0);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a width value by subtracting margin values
/// </summary>
public class WidthMinusMarginConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double width)
        {
            // DefaultUserControlMargin is 8,8,8,8, so subtract 16 (left + right)
            return Math.Max(0, width - 16);
        }
        
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}