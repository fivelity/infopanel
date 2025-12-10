using InfoPanel.Views.Components.WebServer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;

namespace InfoPanel.Views.Components
{
    public sealed partial class Menu : UserControl
    {
        public Menu()
        {
            this.InitializeComponent();
        }

        private async void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            await BeadaPanelTask.Instance.StopAsync();
            Application.Current.Exit();
        }

        private void MenuItemPerformanceSettings_Click(object sender, RoutedEventArgs e)
        {
            var performanceSettings = new PerformanceSettings();

            if (Application.Current is App app)
            {
                if(app.MainWindow != null)
                {
                    performanceSettings.XamlRoot = app.MainWindow.Content.XamlRoot;
                }
            }

            performanceSettings.Show();
        }

        private void MenuItemDiscord_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/cQnjdMC7Qc",
                UseShellExecute = true
            });
        }

        private void MenuItemWebserverSettings_Click(object sender, RoutedEventArgs e)
        {
            var webServerSettings = new WebServerSettings();
            if (Application.Current is App app)
            {
                if (app.MainWindow != null)
                {
                    webServerSettings.XamlRoot = app.MainWindow.Content.XamlRoot;
                }
            }

            webServerSettings.Show();
        }
    }
}
