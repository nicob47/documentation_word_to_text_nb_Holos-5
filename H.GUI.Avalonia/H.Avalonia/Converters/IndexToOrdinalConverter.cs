using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace H.Avalonia.Converters;

/// <summary>
/// Converts an item's position in a collection to an ordinal label (e.g., "1st crop", "2nd crop").
/// Usage as IMultiValueConverter: values[0] = item, values[1] = collection.
/// </summary>
public class IndexToOrdinalConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2 || values[0] is null || values[1] is not IList collection)
            return "—";

        var index = collection.IndexOf(values[0]);
        if (index < 0)
            return "—";

        var position = index + 1;
        var suffix = position switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };

        return $"{position}{suffix} crop";
    }
}
