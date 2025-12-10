using InfoPanel.Models;
using InfoPanel.Monitors;
using InfoPanel.Utils;
using InfoPanel.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using Windows.UI;

namespace InfoPanel.Views.Pages
{
    public sealed partial class SettingsPage : Page
    {
        private static readonly ILogger Logger = Log.ForContext<SettingsPage>();
        public SettingsViewModel ViewModel { get; }

        public SettingsPage()
        {
            ViewModel = new SettingsViewModel();
            this.InitializeComponent();
            InitializeControls();
            LoadSettings();
        }

        public SettingsPage(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            this.InitializeComponent();
            InitializeControls();
            LoadSettings();
        }

        private void InitializeControls()
        {
            // Populate Listen IP
            ComboBoxListenIp.Items.Add("127.0.0.1");
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in interfaces)
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet || ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    IPInterfaceProperties ipProps = ni.GetIPProperties();
                    foreach (IPAddressInformation addr in ipProps.UnicastAddresses)
                    {
                        if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                            && !addr.Address.ToString().StartsWith("169.254."))
                        {
                            ComboBoxListenIp.Items.Add(addr.Address.ToString());
                        }
                    }
                }
            }

            // Populate Listen Port
            ComboBoxListenPort.Items.Add("80");
            ComboBoxListenPort.Items.Add("81");
            ComboBoxListenPort.Items.Add("2020");
            ComboBoxListenPort.Items.Add("8000");
            ComboBoxListenPort.Items.Add("8008");
            ComboBoxListenPort.Items.Add("8080");
            ComboBoxListenPort.Items.Add("8081");
            ComboBoxListenPort.Items.Add("8088");
            ComboBoxListenPort.Items.Add("10000");
            ComboBoxListenPort.Items.Add("10001");

            // Populate Refresh Rate
            ComboBoxRefreshRate.Items.Add(16);
            ComboBoxRefreshRate.Items.Add(33);
            ComboBoxRefreshRate.Items.Add(50);
            ComboBoxRefreshRate.Items.Add(66);
            ComboBoxRefreshRate.Items.Add(100);
            ComboBoxRefreshRate.Items.Add(200);
            ComboBoxRefreshRate.Items.Add(300);
            ComboBoxRefreshRate.Items.Add(500);
            ComboBoxRefreshRate.Items.Add(1000);

            // Wire up events
            ToggleAutoStart.Toggled += (s, e) => ConfigModel.Instance.Settings.AutoStart = ToggleAutoStart.IsOn;
            ToggleMinimizeToTray.Toggled += (s, e) => ConfigModel.Instance.Settings.MinimizeToTray = ToggleMinimizeToTray.IsOn;
            ToggleStartMinimized.Toggled += (s, e) => ConfigModel.Instance.Settings.StartMinimized = ToggleStartMinimized.IsOn;
            ToggleShowGridLines.Toggled += (s, e) => ConfigModel.Instance.Settings.ShowGridLines = ToggleShowGridLines.IsOn;
            ToggleLibreHardwareMonitor.Toggled += (s, e) => ConfigModel.Instance.Settings.LibreHardwareMonitor = ToggleLibreHardwareMonitor.IsOn;
            ToggleWebServer.Toggled += (s, e) => ConfigModel.Instance.Settings.WebServer = ToggleWebServer.IsOn;

            NumberBoxStartupDelay.ValueChanged += (s, e) => ConfigModel.Instance.Settings.AutoStartDelay = (int)NumberBoxStartupDelay.Value;
            NumberBoxGridSpacing.ValueChanged += (s, e) => ConfigModel.Instance.Settings.GridLinesSpacing = (int)NumberBoxGridSpacing.Value;

            SliderFrameRate.ValueChanged += (s, e) =>
            {
                ConfigModel.Instance.Settings.TargetFrameRate = (int)SliderFrameRate.Value;
                RunFrameRate.Text = ((int)SliderFrameRate.Value).ToString();
            };

            SliderGraphUpdateRate.ValueChanged += (s, e) =>
            {
                ConfigModel.Instance.Settings.TargetGraphUpdateRate = (int)SliderGraphUpdateRate.Value;
                RunGraphUpdateRate.Text = ((int)SliderGraphUpdateRate.Value).ToString();
            };

            ComboBoxListenIp.SelectionChanged += (s, e) =>
            {
                if (ComboBoxListenIp.SelectedItem != null)
                    ConfigModel.Instance.Settings.WebServerListenIp = ComboBoxListenIp.SelectedItem.ToString()!;
            };

            ComboBoxListenPort.SelectionChanged += (s, e) =>
            {
                if (ComboBoxListenPort.SelectedItem != null && int.TryParse(ComboBoxListenPort.SelectedItem.ToString(), out int port))
                    ConfigModel.Instance.Settings.WebServerListenPort = port;
            };

            ComboBoxRefreshRate.SelectionChanged += (s, e) =>
            {
                if (ComboBoxRefreshRate.SelectedItem is int rate)
                    ConfigModel.Instance.Settings.WebServerRefreshRate = rate;
            };
        }

        private void LoadSettings()
        {
            var settings = ConfigModel.Instance.Settings;

            ToggleAutoStart.IsOn = settings.AutoStart;
            ToggleMinimizeToTray.IsOn = settings.MinimizeToTray;
            ToggleStartMinimized.IsOn = settings.StartMinimized;
            ToggleShowGridLines.IsOn = settings.ShowGridLines;
            ToggleLibreHardwareMonitor.IsOn = settings.LibreHardwareMonitor;
            ToggleWebServer.IsOn = settings.WebServer;

            NumberBoxStartupDelay.Value = settings.AutoStartDelay;
            NumberBoxGridSpacing.Value = settings.GridLinesSpacing;

            SliderFrameRate.Value = settings.TargetFrameRate;
            SliderGraphUpdateRate.Value = settings.TargetGraphUpdateRate;
            RunFrameRate.Text = settings.TargetFrameRate.ToString();
            RunGraphUpdateRate.Text = settings.TargetGraphUpdateRate.ToString();

            // Select items in combo boxes
            if (!string.IsNullOrEmpty(settings.WebServerListenIp))
            {
                for (int i = 0; i < ComboBoxListenIp.Items.Count; i++)
                {
                    if (ComboBoxListenIp.Items[i]?.ToString() == settings.WebServerListenIp)
                    {
                        ComboBoxListenIp.SelectedIndex = i;
                        break;
                    }
                }
            }

            var portStr = settings.WebServerListenPort.ToString();
            for (int i = 0; i < ComboBoxListenPort.Items.Count; i++)
            {
                if (ComboBoxListenPort.Items[i]?.ToString() == portStr)
                {
                    ComboBoxListenPort.SelectedIndex = i;
                    break;
                }
            }

            for (int i = 0; i < ComboBoxRefreshRate.Items.Count; i++)
            {
                if (ComboBoxRefreshRate.Items[i] is int rate && rate == settings.WebServerRefreshRate)
                {
                    ComboBoxRefreshRate.SelectedIndex = i;
                    break;
                }
            }

            // Update PawniO status
            TextBlockPawnIOStatus.Text = ViewModel.PawnIOStatus;
        }

        private void ButtonOpenDataFolder_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "InfoPanel");
            Process.Start(new ProcessStartInfo("explorer.exe", path) { UseShellExecute = true });
        }

        private async void ButtonCheckPawnIO_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Information("Checking PawniO status from Settings page");
                ViewModel.RefreshPawnIOStatus();
                TextBlockPawnIOStatus.Text = ViewModel.PawnIOStatus;

                if (!PawnIoHelper.IsInstalled || PawnIoHelper.RequiresUpdate)
                {
                    string message = PawnIoHelper.RequiresUpdate
                        ? $"PawniO is outdated (v{PawnIoHelper.Version}).\n\nLibreHardwareMonitor requires PawniO v2.0.0.0 or higher.\n\nWould you like to update it now?"
                        : "PawniO is not installed.\n\nLibreHardwareMonitor requires PawniO for low-level hardware access.\n\nWould you like to install it now?";

                    var dialog = new ContentDialog
                    {
                        Title = "PawniO Driver",
                        Content = message,
                        PrimaryButtonText = "Install",
                        CloseButtonText = "Cancel",
                        XamlRoot = this.XamlRoot
                    };

                    var result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        bool success = PawnIoHelper.InstallOrUpdate();
                        if (success && PawnIoHelper.IsInstalled)
                        {
                            ViewModel.RefreshPawnIOStatus();
                            TextBlockPawnIOStatus.Text = ViewModel.PawnIOStatus;

                            if (ConfigModel.Instance.Settings.LibreHardwareMonitor)
                            {
                                Logger.Information("Restarting LibreMonitor to load PawniO driver");
                                await LibreMonitor.Instance.StopAsync();
                                await LibreMonitor.Instance.StartAsync();
                                Logger.Information("LibreMonitor restarted successfully");
                            }

                            var successDialog = new ContentDialog
                            {
                                Title = "Installation Complete",
                                Content = $"PawniO v{PawnIoHelper.Version} has been installed successfully.\n\nLibreHardwareMonitor has been restarted to load the driver.",
                                CloseButtonText = "OK",
                                XamlRoot = this.XamlRoot
                            };
                            await successDialog.ShowAsync();
                        }
                    }
                }
                else
                {
                    var infoDialog = new ContentDialog
                    {
                        Title = "PawniO Status",
                        Content = $"PawniO v{PawnIoHelper.Version} is installed and up to date.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await infoDialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error checking PawniO status");
                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "An error occurred while checking PawniO status. See logs for details.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }
    }
}
