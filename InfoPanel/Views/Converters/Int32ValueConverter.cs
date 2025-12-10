using System;
using Microsoft.UI.Xaml.Data;

namespace InfoPanel
{
    public class Int32ValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is UInt32 val)
            {
                return "0x" + val.ToString("x");
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
