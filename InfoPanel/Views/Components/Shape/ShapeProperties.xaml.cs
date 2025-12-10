using InfoPanel.Drawing;
using InfoPanel.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using static InfoPanel.Models.ShapeDisplayItem;

namespace InfoPanel.Views.Components
{
    public sealed partial class ShapeProperties : UserControl
    {
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("ShapeDisplayItem", typeof(ShapeDisplayItem), typeof(ShapeProperties));

        public ShapeDisplayItem ShapeDisplayItem
        {
            get { return (ShapeDisplayItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public ShapeProperties()
        {
            this.InitializeComponent();
            ComboBoxType.ItemsSource = Enum.GetValues(typeof(ShapeType)).Cast<ShapeType>();
            ComboBoxGradientType.ItemsSource = Enum.GetValues(typeof(GradientType)).Cast<GradientType>();
        }
    }
}
