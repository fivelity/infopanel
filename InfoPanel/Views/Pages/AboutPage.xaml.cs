using InfoPanel.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace InfoPanel.Views.Pages
{
    /// <summary>
    /// About page for InfoPanel WinUI 3 application
    /// </summary>
    public sealed partial class AboutPage : Page
    {
        public AboutViewModel ViewModel { get; }

        public AboutPage()
        {
            ViewModel = new AboutViewModel();
            this.InitializeComponent();
        }

        public AboutPage(AboutViewModel viewModel)
        {
            ViewModel = viewModel;
            this.InitializeComponent();
        }
    }
}
