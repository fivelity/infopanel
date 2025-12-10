using InfoPanel.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace InfoPanel.Views.Components
{
    public sealed partial class GroupProperties : UserControl
    {
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("GroupDisplayItem", typeof(GroupDisplayItem), typeof(GroupProperties));

        public GroupDisplayItem GroupDisplayItem
        {
            get { return (GroupDisplayItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }
        
        public GroupProperties()
        {
            this.InitializeComponent();
        }
    }
}
