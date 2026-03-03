using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace H.Avalonia.Converters;

/// <summary>
/// Converts an enum value to a boolean for radio button binding.
/// The converter parameter specifies which enum value should return true.
/// 
/// Usage in XAML:
/// <RadioButton IsChecked="{Binding MyEnumProperty, 
///              Converter={StaticResource EnumToBoolConverter}, 
///              ConverterParameter={x:Static enums:MyEnum.Value1}}"/>
/// </summary>
public class EnumToBoolConverter : IValueConverter
{
    /// <summary>
    /// Converts an enum value to a boolean.
    /// Returns true if the value matches the parameter, false otherwise.
    /// </summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
        {
            return false;
        }

        // Both value and parameter should be the same enum type
        // Return true if they match, false otherwise
        return value.Equals(parameter);
    }

    /// <summary>
    /// Converts a boolean back to an enum value.
    /// Returns the parameter value if the boolean is true, otherwise returns the original value.
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue && parameter != null)
        {
            // Radio button was checked, return the parameter enum value
            return parameter;
        }

        // Return BindingOperations.DoNothing to indicate we don't want to update the binding
        return BindingOperations.DoNothing;
    }
}

