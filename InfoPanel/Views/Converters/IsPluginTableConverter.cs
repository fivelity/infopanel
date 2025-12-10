using InfoPanel.Plugins;
using System;
using System.Data;
using Microsoft.UI.Xaml.Data;

namespace InfoPanel
{
    public class IsPluginTableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is IPluginTable pluginTable && pluginTable.Value is DataTable;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
