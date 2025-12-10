using InfoPanel.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace InfoPanel.Views.Components
{
    public sealed partial class TableSensorProperties : UserControl
    {
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("TableSensorDisplayItem", typeof(TableSensorDisplayItem), typeof(TableSensorProperties));

        public TableSensorDisplayItem TableSensorDisplayItem
        {
            get { return (TableSensorDisplayItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public TableSensorProperties()
        {
            this.InitializeComponent();
        }
    }
}
