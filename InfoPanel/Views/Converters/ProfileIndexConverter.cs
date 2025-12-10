using InfoPanel.Models;
using System;
using Microsoft.UI.Xaml.Data;

namespace InfoPanel
{
    public class ProfileIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Profile profile)
            {
                return ConfigModel.Instance.Profiles.IndexOf(profile);
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
