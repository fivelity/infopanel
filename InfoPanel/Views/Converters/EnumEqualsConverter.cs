using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace InfoPanel
{
    public class EnumEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            string? parameterString = parameter.ToString();
            string? valueString = value.ToString();

            return string.Equals(valueString, parameterString, StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
