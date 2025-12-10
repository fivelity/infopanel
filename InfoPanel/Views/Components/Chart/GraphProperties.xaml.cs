using InfoPanel.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using static InfoPanel.Models.GraphDisplayItem;

namespace InfoPanel.Views.Components
{
    public sealed partial class GraphProperties : UserControl
    {
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("GraphDisplayItem", typeof(GraphDisplayItem), typeof(GraphProperties));

        public GraphDisplayItem GraphDisplayItem
        {
            get { return (GraphDisplayItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public GraphProperties()
        {
            this.InitializeComponent();
            ComboBoxType.ItemsSource = Enum.GetValues(typeof(GraphType)).Cast<GraphType>();
        }

    }
}
