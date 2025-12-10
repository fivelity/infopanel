using InfoPanel.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Linq;
using static InfoPanel.Models.SensorDisplayItem;

namespace InfoPanel.Views.Components
{
    public sealed partial class SensorProperties : UserControl
    {
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("SensorDisplayItem", typeof(SensorDisplayItem), typeof(SensorProperties));

        public SensorDisplayItem SensorDisplayItem
        {
            get { return (SensorDisplayItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public SensorProperties()
        {
            this.InitializeComponent();
            ComboBoxValueType.ItemsSource = Enum.GetValues(typeof(SensorValueType)).Cast<SensorValueType>();
        }

        private void NumberBoxPrecision_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SensorDisplayItem == null)
            {
                return;
            }

            var numBox = ((NumberBox)sender);
            if (double.TryParse(numBox.Text, out double newValue))
            {
                numBox.Value = newValue;
                SensorDisplayItem.Precision = (int)newValue;
            }
        }

        private void NumberBoxThreshold1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SensorDisplayItem == null)
            {
                return;
            }

            var numBox = ((NumberBox)sender);
            if (double.TryParse(numBox.Text, out double newValue))
            {
                numBox.Value = newValue;
                SensorDisplayItem.Threshold1 = newValue;
            }
        }

        private void NumberBoxThreshold2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SensorDisplayItem == null)
            {
                return;
            }

            var numBox = ((NumberBox)sender);
            if (double.TryParse(numBox.Text, out double newValue))
            {
                numBox.Value = newValue;
                SensorDisplayItem.Threshold2 = newValue;
            }
        }

        private void NumberBoxAddition_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SensorDisplayItem == null)
            {
                return;
            }

            var numBox = ((NumberBox)sender);
            if (double.TryParse(numBox.Text, out double newValue))
            {
                numBox.Value = newValue;
                SensorDisplayItem.AdditionModifier = newValue;
            }
        }

        // Note: Key event handling for NumberBox multiplication field is disabled
        // as it relied on WPF-specific event patterns. This functionality can be
        // re-implemented using WinUI 3 controls if needed in the future.
        //        e.Handled = true;
        //    }
        //}
    }
}
