using InfoPanel.Models;
using InfoPanel.Views.Components.Custom;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Serilog;
using System;
using System.IO;
using Windows.Storage.Pickers;
using WinRT.Interop;
using InfoPanel.App;

namespace InfoPanel.Views.Components
{
    public sealed partial class CustomProperties : UserControl
    {
        private static readonly ILogger Logger = Log.ForContext<CustomProperties>();
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("GaugeDisplayItem", typeof(GaugeDisplayItem), typeof(CustomProperties));

        public GaugeDisplayItem GaugeDisplayItem
        {
            get { return (GaugeDisplayItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        //to fix swapping view not refreshing when items empty etc
        public GaugePropertiesVM ViewModel { get; set; }

        private Microsoft.UI.Xaml.DispatcherTimer UpdateTimer;

        public CustomProperties()
        {
            ViewModel= new GaugePropertiesVM();

            InitializeComponent();
            Unloaded += CustomProperties_Unloaded;

            UpdateTimer = new() { Interval = TimeSpan.FromMilliseconds(100) };
            UpdateTimer.Tick += Timer_Tick;
            UpdateTimer.Start();
        }

        private void CustomProperties_Unloaded(object sender, RoutedEventArgs e)
        {
            if (UpdateTimer != null)
            {
                UpdateTimer.Stop();
                UpdateTimer.Tick -= Timer_Tick;
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (SharedModel.Instance.SelectedItem is GaugeDisplayItem gaugeDisplayItem)
            {
                if (gaugeDisplayItem.Images.Count > 0)
                {
                    gaugeDisplayItem.TriggerDisplayImageChange();
                }
            }
        }

        private async void ButtonAddStep_Click(object sender, RoutedEventArgs e)
        {
            if (SharedModel.Instance.SelectedItem is GaugeDisplayItem customDisplayItem)
            {
                var window = App.MainWindow;
                if (window == null) return;
                
                var filePicker = new FileOpenPicker
                {
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                    ViewMode = PickerViewMode.Thumbnail
                };
                filePicker.FileTypeFilter.Add(".jpg");
                filePicker.FileTypeFilter.Add(".jpeg");
                filePicker.FileTypeFilter.Add(".png");
                filePicker.FileTypeFilter.Add(".gif");
                
                var hwnd = WindowNative.GetWindowHandle(window);
                InitializeWithWindow.Initialize(filePicker, hwnd);
                
                var files = await filePicker.PickMultipleFilesAsync();
                if (files.Count > 0)
                {
                    if (files.Count > 101)
                    {
                        await ShowErrorDialog("You can only select a maximum of 101 images.", "File Selection Error");
                        return;
                    }

                    var profile = SharedModel.Instance.SelectedProfile;

                    if (profile != null)
                    {
                        customDisplayItem.Images.Clear();

                        foreach (var file in files)
                        {
                            var imageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "InfoPanel", "assets", profile.Guid.ToString());
                            if (!Directory.Exists(imageFolder))
                            {
                                Directory.CreateDirectory(imageFolder);
                            }

                            try
                            {
                                var filePath = file.Path;
                                var fileName = file.Name;
                                File.Copy(filePath, Path.Combine(imageFolder, fileName), true);
                                var imageDisplayItem = new ImageDisplayItem(fileName, profile, fileName, true)
                                {
                                    PersistentCache = true // Gauge images should not expire
                                };

                                customDisplayItem.Images.Add(imageDisplayItem);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex, "Error copying file to assets folder");
                            }
                        }
                    }
                }
            }
        }
        
        private async System.Threading.Tasks.Task ShowErrorDialog(string message, string title)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private void ButtonStepUp_Click(object sender, RoutedEventArgs e)
        {
            if (SharedModel.Instance.SelectedItem is GaugeDisplayItem gaugeDisplayItem)
            {
                if (ViewModel.SelectedItem != null)
                {
                    var index = gaugeDisplayItem.Images.IndexOf(ViewModel.SelectedItem);
                    if (index > 0)
                    {
                        var selectedItem = ViewModel.SelectedItem;
                        var temp = gaugeDisplayItem.Images[index - 1];
                        gaugeDisplayItem.Images[index - 1] = gaugeDisplayItem.Images[index];
                        gaugeDisplayItem.Images[index] = temp;
                        // Items refresh automatically in WinUI 3 via ObservableCollection
                        ViewModel.SelectedItem = selectedItem;
                        ListViewItems.ScrollIntoView(selectedItem);
                    }
                }
            }
        }

        private void ButtonStepDown_Click(object sender, RoutedEventArgs e)
        {
            if (SharedModel.Instance.SelectedItem is GaugeDisplayItem gaugeDisplayItem)
            {
                if (ViewModel.SelectedItem != null)
                {
                    var index = gaugeDisplayItem.Images.IndexOf(ViewModel.SelectedItem);
                    if (index < gaugeDisplayItem.Images.Count - 1)
                    {
                        var selectedItem = ViewModel.SelectedItem;
                        var temp = gaugeDisplayItem.Images[index + 1];
                        gaugeDisplayItem.Images[index + 1] = gaugeDisplayItem.Images[index];
                        gaugeDisplayItem.Images[index] = temp;
                        // Items refresh automatically in WinUI 3 via ObservableCollection
                        ViewModel.SelectedItem = selectedItem;
                        ListViewItems.ScrollIntoView(selectedItem);
                    }
                }
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (SharedModel.Instance.SelectedItem is GaugeDisplayItem gaugeDisplayItem)
            {
                if (ViewModel.SelectedItem != null)
                {
                    //customDisplayItem.Images.Remove(ViewModel.SelectedItem);
                    for (int i = gaugeDisplayItem.Images.Count - 1; i >= 0; i--)
                    {
                        if (gaugeDisplayItem.Images[i].Selected)
                        {
                            gaugeDisplayItem.Images.RemoveAt(i);
                        }
                    }
                    // Items refresh automatically in WinUI 3 via ObservableCollection
                    gaugeDisplayItem.TriggerDisplayImageChange();
                }
            }
        }


    }
}
