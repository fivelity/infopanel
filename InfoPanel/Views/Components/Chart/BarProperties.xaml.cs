using InfoPanel.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace InfoPanel.Views.Components
{
    public sealed partial class BarProperties : UserControl
    {
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("BarDisplayItem", typeof(BarDisplayItem), typeof(BarProperties));

        public BarDisplayItem BarDisplayItem
        {
            get { return (BarDisplayItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public BarProperties()
        {
            InitializeComponent();
        }
    }
}
