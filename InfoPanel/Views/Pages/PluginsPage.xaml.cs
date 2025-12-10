using InfoPanel.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using Windows.Storage.Pickers;

namespace InfoPanel.Views.Pages
{
    public sealed partial class PluginsPage : Page
    {
        public PluginsViewModel ViewModel { get; }

        public PluginsPage()
        {
            ViewModel = new PluginsViewModel();
            this.InitializeComponent();
            LoadPlugins();
        }

        public PluginsPage(PluginsViewModel viewModel)
        {
            ViewModel = viewModel;
            this.InitializeComponent();
            LoadPlugins();
        }

        private void LoadPlugins()
        {
            RepeaterBundledPlugins.ItemsSource = ViewModel.BundledPlugins;
            RepeaterExternalPlugins.ItemsSource = ViewModel.ExternalPlugins;

            // Update visibility of empty state
            BorderNoUserPlugins.Visibility = ViewModel.ExternalPlugins.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            // Update restart banner
            ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ViewModel.ShowRestartBanner))
                {
                    BorderRestartBanner.Visibility = ViewModel.ShowRestartBanner ? Visibility.Visible : Visibility.Collapsed;
                }
            };
        }

        private void OpenPluginsFolder_Click(object sender, RoutedEventArgs e)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "InfoPanel", "plugins");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            Process.Start(new ProcessStartInfo("explorer.exe", path) { UseShellExecute = true });
        }

        private async void ImportPlugin_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".zip");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.GetService<Views.Windows.MainWindow>());
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                await ViewModel.AddPluginFromZipAsync(file.Path);
                LoadPlugins();
            }
        }
    }
}
