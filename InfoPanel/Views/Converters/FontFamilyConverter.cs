using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace InfoPanel
{
    public class FontFamilyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string fontFamilyName)
            {
                return new FontFamily(fontFamilyName);
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is FontFamily fontFamily)
            {
                return fontFamily.Source;
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
