using InfoPanel.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using Windows.ApplicationModel.DataTransfer;

namespace InfoPanel.Views.Components
{
    public sealed partial class DisplayItemView : UserControl
    {
        public DisplayItemView()
        {
            this.InitializeComponent();
            this.Loaded += DisplayItemView_Loaded;
        }

        private void DisplayItemView_Loaded(object sender, RoutedEventArgs e)
        {
            // Enable drag and drop
            this.CanDrag = true;
            this.DragStarting += DisplayItemView_DragStarting;
        }

        private void DisplayItemView_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            if (DataContext is DisplayItem item)
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText(item.Name);
                args.Data = dataPackage;
            }
        }

        public string CoordinatesText => $"{X}, {Y}";

        public string Kind => (DataContext as DisplayItem)?.Kind ?? string.Empty;

        public string SensorType => (DataContext as DisplayItem)?.SensorType ?? string.Empty;

        public bool IsLocked => (DataContext as DisplayItem)?.IsLocked ?? false;

        public bool Hidden => (DataContext as DisplayItem)?.Hidden ?? false;

        public int X => (DataContext as DisplayItem)?.X ?? 0;

        public int Y => (DataContext as DisplayItem)?.Y ?? 0;
    }

    public class KindToGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string kind = value as string;
            return kind switch
            {
                "Text" => "\uE8A5",
                "Image" => "\uE91B",
                "Sensor" => "\uE7C6",
                "Table" => "\uE8CE",
                "Clock" => "\uE917",
                "Calendar" => "\uE787",
                "Graph" => "\uE9D2",
                "Bar" => "\uE9D3",
                "Donut" => "\uE9D4",
                "Gauge" => "\uE9D1",
                "Sensor Image" => "\uE91B",
                "Http Image" => "\uE774",
                "Shape" => "\uEA6F",
                _ => "\uE8A5"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class SensorTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string sensorType = value as string;
            return sensorType switch
            {
                "HwInfo" => new Uri("ms-appx:///Resources/Images/hwinfo_icon.ico"),
                "Libre" => new Uri("ms-appx:///Resources/Images/libre_icon.ico"),
                "Plugin" => new Uri("ms-appx:///Resources/Images/logo.ico"),
                _ => new Uri("ms-appx:///Resources/Images/logo.ico")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
