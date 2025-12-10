using InfoPanel.Models;
using InfoPanel.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace InfoPanel.Views.Components
{
    public sealed partial class PluginProperties : UserControl
    {
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("PluginDisplayModel", typeof(PluginViewModel), typeof(PluginProperties));

        public PluginViewModel PluginDisplayModel
        {
            get { return (PluginViewModel)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public PluginProperties()
        {
            InitializeComponent();
        }
    }
}
