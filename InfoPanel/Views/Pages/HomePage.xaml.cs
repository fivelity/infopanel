using InfoPanel.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace InfoPanel.Views.Pages
{
    /// <summary>
    /// Home page for InfoPanel WinUI 3 application
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomeViewModel? ViewModel { get; }

        public HomePage()
        {
            this.InitializeComponent();
        }

        public HomePage(HomeViewModel viewModel)
        {
            ViewModel = viewModel;
            this.InitializeComponent();
        }

        private void NavigateButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                var frame = this.Frame;
                if (frame == null) return;

                switch (tag)
                {
                    case "profiles":
                        frame.Navigate(typeof(ProfilesPage));
                        break;
                    case "design":
                        frame.Navigate(typeof(DesignPage));
                        break;
                    case "updates":
                        frame.Navigate(typeof(UpdatesPage));
                        break;
                    case "plugins":
                        frame.Navigate(typeof(PluginsPage));
                        break;
                    case "settings":
                        frame.Navigate(typeof(SettingsPage));
                        break;
                    case "about":
                        frame.Navigate(typeof(AboutPage));
                        break;
                }
            }
        }
    }
}
