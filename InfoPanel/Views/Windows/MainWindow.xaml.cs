using System.Reflection;
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using InfoPanel.Utils;
using InfoPanel.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InfoPanel.Views.Windows
{
    /// <summary>
    /// Main window for InfoPanel WinUI 3 application
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly INavigationService? _navigationService;

        public MainWindow()
        {
            this.InitializeComponent();

            _navigationService = App.GetService<INavigationService>();

            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);
            if (version != null)
            {
                AppTitleTextBlock.Text = $"InfoPanel - v{version}";
                Title = $"InfoPanel - v{version}";
            }

            // Set up custom title bar
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            // Navigate to home page on startup
            ContentFrame.Navigate(typeof(Pages.HomePage));
            
            // Select the first item in navigation
            if (RootNavigation.MenuItems.Count > 0)
            {
                RootNavigation.SelectedItem = RootNavigation.MenuItems[0];
            }

            Closed += MainWindow_Closed;
        }

        private void RootNavigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer is NavigationViewItem item && item.Tag is string tag)
            {
                Type? pageType = tag switch
                {
                    "home" => typeof(Pages.HomePage),
                    "profiles" => typeof(Pages.ProfilesPage),
                    "design" => typeof(Pages.DesignPage),
                    "updates" => typeof(Pages.UpdatesPage),
                    "plugins" => typeof(Pages.PluginsPage),
                    "usbpanels" => typeof(Pages.UsbPanelsPage),
                    "settings" => typeof(Pages.SettingsPage),
                    "about" => typeof(Pages.AboutPage),
                    _ => null
                };

                if (pageType != null)
                {
                    ContentFrame.Navigate(pageType);
                }
            }
        }

        public bool Navigate(Type pageType)
        {
            return ContentFrame.Navigate(pageType);
        }

        private async void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            await FileUtil.CleanupAssets();
            await App.CleanShutDown();
        }
    }
}
