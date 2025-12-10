using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace InfoPanel
{
    public class IntDoubleValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (double.TryParse(value?.ToString(), out double doubleValue))
            {
                return doubleValue;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double doubleValue)
            {
                return (int)doubleValue;
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
