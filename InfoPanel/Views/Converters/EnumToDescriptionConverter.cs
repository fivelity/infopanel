using InfoPanel.Extensions;
using System;
using Microsoft.UI.Xaml.Data;

namespace InfoPanel
{
    public class EnumToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Enum enumValue)
            {
                return enumValue.GetDescription();
            }
            return value?.ToString() ?? "-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
