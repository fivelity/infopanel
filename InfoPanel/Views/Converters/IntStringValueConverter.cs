using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace InfoPanel
{
    public class IntStringValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int intValue)
            {
                return intValue.ToString();
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string stringValue && int.TryParse(stringValue, out int intValue))
            {
                return intValue;
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
