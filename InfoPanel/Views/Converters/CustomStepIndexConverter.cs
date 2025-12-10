using InfoPanel.Models;
using System;
using Microsoft.UI.Xaml.Data;

namespace InfoPanel
{
    public class CustomStepIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ImageDisplayItem imageDisplayItem)
            {
                if (SharedModel.Instance.SelectedItem is GaugeDisplayItem customDisplayItem)
                {
                    var step = 100.0 / (customDisplayItem.Images.Count - 1);
                    var index = customDisplayItem.Images.IndexOf(imageDisplayItem);

                    int endValue = (int)Math.Ceiling(index * step) + (int)step;

                    if (index == customDisplayItem.Images.Count - 1)
                    {
                        return "≤100%";
                    }
                    else
                    {
                        return $"≤{endValue - 1}%";
                    }
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
