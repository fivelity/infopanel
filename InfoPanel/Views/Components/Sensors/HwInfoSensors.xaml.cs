using InfoPanel.Models;
using InfoPanel.ViewModels.Components;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Diagnostics;

namespace InfoPanel.Views.Components
{
    public sealed partial class HwInfoSensors : UserControl
    {
        private HwInfoSensorsVM ViewModel { get; set; }

        private readonly Microsoft.UI.Xaml.DispatcherTimer UpdateTimer = new() { Interval = TimeSpan.FromSeconds(1) };

        public HwInfoSensors()
        {
            ViewModel = new HwInfoSensorsVM();
            DataContext = ViewModel;

            this.InitializeComponent();

            Loaded += HwInfoSensors_Loaded;
            Unloaded += HwInfoSensors_Unloaded;
        }

        private void HwInfoSensors_Loaded(object sender, RoutedEventArgs e)
        {
            if (UpdateTimer != null)
            {
                UpdateTimer.Tick += Timer_Tick;

                Timer_Tick(this, new object());
                UpdateTimer.Start();
            }
        }

        private void HwInfoSensors_Unloaded(object sender, RoutedEventArgs e)
        {
            if (UpdateTimer != null)
            {
                UpdateTimer.Stop();
                UpdateTimer.Tick -= Timer_Tick;
            }
        }

        private void Timer_Tick(object? sender, object e)
        {
            LoadSensorTree();
            UpdateSensorDetails();
        }

        private void LoadSensorTree()
        {
            foreach (HWHash.HWINFO_HASH hash in HWHash.GetOrderedList())
            {
                //construct parent
                var parent = ViewModel.FindParentSensorItem(hash.ParentUniqueID);
                if(parent == null)
                {
                    parent = new TreeItem(hash.ParentUniqueID, hash.ParentNameDefault);
                    ViewModel.Sensors.Add(parent);
                }

                TreeItem? group;
                if(hash.ReadingType != "Other" && hash.ReadingType != "None")
                {
                    //construct type grouping
                    group = parent.FindChild(hash.ReadingType);

                    if (group == null)
                    {
                        group = new TreeItem(hash.ReadingType, hash.ReadingType);
                        parent.Children.Add(group);
                    }
                } else
                {
                    group = parent;
                }

                //construct actual sensor
                var child = group.FindChild(hash.UniqueID);
                if (child == null)
                {
                    child = new HwInfoSensorItem(hash.UniqueID, hash.NameDefault, hash.ParentID, hash.ParentInstance, hash.SensorID);
                    group.Children.Add(child);
                }
            }
        }

        private void TreeViewInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is HwInfoSensorItem sensorItem)
            {
                ViewModel.SelectedItem = sensorItem;
                sensorItem.Update();
            }
            else
            {
                ViewModel.SelectedItem = null;
            }
        }

        private void ScrollViewer_PointerWheelChanged(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            var delta = e.GetCurrentPoint(scrollViewer).Properties.MouseWheelDelta;
            scrollViewer.ChangeView(null, scrollViewer.VerticalOffset - delta, null);
            e.Handled = true;
        }

        private void UpdateSensorDetails()
        {
            ViewModel.SelectedItem?.Update();
        }


        private void ImageLogo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "explorer.exe";
            process.StartInfo.Arguments = "https://www.hwinfo.com/";
            process.Start();
        }
    }
}
