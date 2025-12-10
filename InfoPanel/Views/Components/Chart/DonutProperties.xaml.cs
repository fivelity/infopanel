using InfoPanel.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace InfoPanel.Views.Components
{
    public sealed partial class DonutProperties : UserControl
    {
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("DonutDisplayItem", typeof(DonutDisplayItem), typeof(DonutProperties));

        public DonutDisplayItem DonutDisplayItem
        {
            get { return (DonutDisplayItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public DonutProperties()
        {
            InitializeComponent();
        }
    }
}
