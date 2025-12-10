using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace InfoPanel
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool bValue = false;
            if (value is bool b)
            {
                bValue = b;
            }
            else if (value is bool? nullable && nullable.HasValue)
            {
                bValue = nullable.Value;
            }
            return bValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }
}
