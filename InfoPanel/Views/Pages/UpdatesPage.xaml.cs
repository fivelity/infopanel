using Flurl;
using Flurl.Http;
using InfoPanel.Models;
using InfoPanel.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace InfoPanel.Views.Pages
{
    public sealed partial class UpdatesPage : Page
    {
        public UpdatesViewModel ViewModel { get; }

        public UpdatesPage()
        {
            ViewModel = new UpdatesViewModel();
            this.InitializeComponent();
            InitializePage();
            CheckUpdates();
        }

        public UpdatesPage(UpdatesViewModel viewModel)
        {
            ViewModel = viewModel;
            this.InitializeComponent();
            InitializePage();
            CheckUpdates();
        }

        private void InitializePage()
        {
            TextBlockVersion.Text = $"You are using InfoPanel v{ViewModel.Version}";
            RepeaterVersions.ItemsSource = ViewModel.UpdateVersions;
        }

        private void ButtonCheckUpdates_Click(object sender, RoutedEventArgs _)
        {
            CheckUpdates();
        }

        private async void CheckUpdates()
        {
            ViewModel.UpdateCheckInProgress = true;
            ButtonCheckUpdates.IsEnabled = false;
            TextBlockUpdateStatus.Text = "Checking for updates...";

            try
            {
                var latestVersion = await "https://update.infopanel.net"
                    .AppendPathSegment("latest")
                    .GetAsync()
                    .ReceiveJson<VersionModel>();

                await Task.Delay(500);

                if (IsNewerVersionAvailable(ViewModel.Version, latestVersion.Version))
                {
                    ViewModel.VersionModel = latestVersion;
                    ViewModel.UpdateAvailable = true;
                    TextBlockUpdateStatus.Text = $"Version {latestVersion.Version} is available. Click update to download and install.";
                    ButtonUpdate.Visibility = Visibility.Visible;
                }
                else
                {
                    ViewModel.UpdateAvailable = false;
                    TextBlockUpdateStatus.Text = "There are no updates available. You are using the latest version.";
                    ButtonUpdate.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
                TextBlockUpdateStatus.Text = "Failed to check for updates. Please try again later.";
            }

            ViewModel.UpdateCheckInProgress = false;
            ButtonCheckUpdates.IsEnabled = true;
        }

        private static bool IsNewerVersionAvailable(string currentVersion, string newVersion)
        {
            Version current = Version.Parse(currentVersion);
            Version latest = Version.Parse(newVersion);
            return latest > current;
        }

        private async void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.VersionModel?.Url is string url)
            {
                ViewModel.DownloadInProgress = true;
                ViewModel.DownloadProgress = 0;
                ButtonUpdate.IsEnabled = false;
                ProgressBarDownload.Visibility = Visibility.Visible;

                var cts = new CancellationTokenSource();
                IProgress<DownloadProgressArgs> progressReporter = new Progress<DownloadProgressArgs>(args =>
                {
                    ViewModel.DownloadProgress = args.PercentComplete;
                    ProgressBarDownload.Value = args.PercentComplete;
                });

                using (var stream = await DownloadStreamWithProgressAsync(url, cts.Token, progressReporter))
                {
                    try
                    {
                        var downloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "InfoPanel", "updates");
                        if (!Directory.Exists(downloadPath))
                        {
                            Directory.CreateDirectory(downloadPath);
                        }

                        var filePath = Path.Combine(downloadPath, "InfoPanelSetup.exe");
                        SaveStreamToFile(stream, filePath);

                        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                        Environment.Exit(0);
                    }
                    catch { }
                }

                ViewModel.DownloadInProgress = false;
                ButtonUpdate.IsEnabled = true;
                ProgressBarDownload.Visibility = Visibility.Collapsed;
            }
        }

        private static void SaveStreamToFile(Stream stream, string filePath)
        {
            using var fileStream = new FileStream(filePath, FileMode.Create);
            stream.CopyTo(fileStream);
        }

        private static async Task<Stream> DownloadStreamWithProgressAsync(string url, CancellationToken cancellationToken, IProgress<DownloadProgressArgs> progressReporter)
        {
            using IFlurlResponse response = await url.GetAsync(HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            using var stream = await response.GetStreamAsync();
            var receivedBytes = 0;
            var buffer = new byte[4096];
            var totalBytes = Convert.ToDouble(response.ResponseMessage.Content.Headers.ContentLength);

            var memStream = new MemoryStream();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                await memStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);

                if (bytesRead == 0) break;

                receivedBytes += bytesRead;
                progressReporter.Report(new DownloadProgressArgs(receivedBytes, totalBytes));
            }

            memStream.Position = 0;
            return memStream;
        }

        public class DownloadProgressArgs : EventArgs
        {
            public DownloadProgressArgs(int bytesReceived, double totalBytes)
            {
                BytesReceived = bytesReceived;
                TotalBytes = totalBytes;
            }

            public double TotalBytes { get; }
            public double BytesReceived { get; }
            public double PercentComplete => 100 * (BytesReceived / TotalBytes);
        }
    }
}
