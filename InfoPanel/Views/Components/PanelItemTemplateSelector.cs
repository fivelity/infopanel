using InfoPanel.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace InfoPanel.Views.Components
{
    public class PanelItemTemplateSelector: DataTemplateSelector
    {
        public required DataTemplate SimpleTemplate { get; set; }

        public required DataTemplate GroupTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return item switch
            {
                GroupDisplayItem => GroupTemplate,
                _ => SimpleTemplate
            };
        }
    }
}
