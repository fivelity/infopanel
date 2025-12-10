using InfoPanel.Models;
using System;
using Microsoft.UI.Xaml.Data;

namespace InfoPanel
{
    public class SelectedItemTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DisplayItem displayItem)
            {
                return $"{displayItem.Kind} Properties";
            }
            else if (value is Profile profile)
            {
                return profile.Name;
            }
            return "No item selected";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
