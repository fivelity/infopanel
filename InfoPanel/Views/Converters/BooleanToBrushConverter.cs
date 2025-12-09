using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace InfoPanel
{
    /// <summary>
    /// Converter that returns a brush based on a boolean value.
    /// True = Accent color brush, False = Transparent
    /// </summary>
    public class BooleanToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
            {
                // Return accent color when true
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0078D4"));
            }

            // Return transparent when false
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
