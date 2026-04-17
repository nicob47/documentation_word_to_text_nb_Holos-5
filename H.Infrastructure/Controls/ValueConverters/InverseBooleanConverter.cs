using Avalonia.Data.Converters;

namespace H.Infrastructure.Controls.ValueConverters
{
    /// <summary>
    /// Takes a boolean value and returns the inverse of that value.
    /// </summary>
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object? Convert(object? value, Type targetType, object? parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return value is bool b && !b;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
