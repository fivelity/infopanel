using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace InfoPanel.Views.Converters
{
    public class LockColorConverter : IValueConverter
    {
        private static readonly SolidColorBrush GrayBrush = new(Colors.Gray);
        private static readonly SolidColorBrush DodgerBlueBrush = new(Colors.DodgerBlue);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool isLocked && isLocked)
                ? GrayBrush
                : DodgerBlueBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
