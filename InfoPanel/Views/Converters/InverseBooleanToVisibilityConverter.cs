using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace InfoPanel
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool bValue = false;
            if (value is bool b)
            {
                bValue = b;
            }
            else if (value is bool? tmp)
            {
                bValue = tmp ?? false;
            }
            return bValue ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
