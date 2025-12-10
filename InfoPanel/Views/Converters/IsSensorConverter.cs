using InfoPanel.Models;
using System;
using Microsoft.UI.Xaml.Data;

namespace InfoPanel
{
    public class IsSensorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return SharedModel.Instance.SelectedItem is SensorDisplayItem
                || SharedModel.Instance.SelectedItem is TableSensorDisplayItem
                || SharedModel.Instance.SelectedItem is ChartDisplayItem
                || SharedModel.Instance.SelectedItem is GaugeDisplayItem
                || SharedModel.Instance.SelectedItem is SensorImageDisplayItem
                || SharedModel.Instance.SelectedItem is HttpImageDisplayItem;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
