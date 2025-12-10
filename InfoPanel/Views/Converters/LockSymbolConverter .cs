using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace InfoPanel.Views.Converters
{
    public class LockSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool isLocked && isLocked)
                ? Symbol.ProtectedDocument
                : Symbol.Document;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
