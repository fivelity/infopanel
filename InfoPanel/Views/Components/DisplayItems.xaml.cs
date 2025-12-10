using InfoPanel.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Core;

namespace InfoPanel.Views.Components
{
    public sealed partial class DisplayItems : UserControl
    {
        private static readonly ILogger Logger = Log.ForContext<DisplayItems>();
        private static DisplayItem? SelectedItem => SharedModel.Instance.SelectedItem;

        private string _searchText = string.Empty;
        private ObservableCollection<DisplayItem> _filteredDisplayItems = new();

        public ObservableCollection<DisplayItem> FilteredDisplayItems => _filteredDisplayItems;

        public DisplayItems()
        {
            this.InitializeComponent();
            this.Loaded += DisplayItems_Loaded;
            this.Unloaded += DisplayItems_Unloaded;
        }

        private void DisplayItems_Loaded(object sender, RoutedEventArgs e)
        {
            SharedModel.Instance.PropertyChanged += Instance_PropertyChanged;
            RefreshFilteredItems();
        }

        private void DisplayItems_Unloaded(object sender, RoutedEventArgs e)
        {
            SharedModel.Instance.PropertyChanged -= Instance_PropertyChanged;
        }

        private void Instance_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SharedModel.Instance.SelectedItem))
            {
                Logger.Debug("SelectedItem changed");
                if (SelectedItem != null)
                {
                    var group = SharedModel.Instance.GetParent(SelectedItem);

                    if (group is not null)
                    {
                        if (!group.IsExpanded)
                        {
                            ListViewItems.ScrollIntoView(group);
                            group.IsExpanded = true;
                        }
                    }
                }
            }
            else if (e.PropertyName == nameof(SharedModel.Instance.SelectedProfile) ||
                     e.PropertyName == nameof(SharedModel.Instance.DisplayItems))
            {
                RefreshFilteredItems();
            }
        }

        private void RefreshFilteredItems()
        {
            _filteredDisplayItems.Clear();
            
            var allItems = SharedModel.Instance.DisplayItems;
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                foreach (var item in allItems)
                {
                    _filteredDisplayItems.Add(item);
                }
            }
            else
            {
                var searchLower = _searchText.ToLower();
                var searchTerms = searchLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var item in allItems)
                {
                    if (item is GroupDisplayItem groupItem)
                    {
                        bool groupMatches = MatchesAllTerms(groupItem.Name, searchTerms);
                        bool hasMatchingChildren = !groupMatches && groupItem.DisplayItems.Any(child =>
                            MatchesAllTerms(child.Name, searchTerms) ||
                            MatchesAllTerms(child.Kind, searchTerms));

                        if (groupMatches || hasMatchingChildren)
                        {
                            _filteredDisplayItems.Add(item);
                            if (hasMatchingChildren)
                            {
                                groupItem.IsExpanded = true;
                            }
                        }
                    }
                    else
                    {
                        if (MatchesAllTerms(item.Name, searchTerms) ||
                            MatchesAllTerms(item.Kind, searchTerms))
                        {
                            _filteredDisplayItems.Add(item);
                        }
                    }
                }
            }
        }

        private static bool MatchesAllTerms(string text, string[] searchTerms)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            var textLower = text.ToLower();
            return searchTerms.All(term => textLower.Contains(term));
        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchText = TextBoxSearch.Text ?? string.Empty;
            RefreshFilteredItems();
        }



        private void ScrollToView(DisplayItem displayItem)
        {
            var group = SharedModel.Instance.GetParent(displayItem);

            if (group is GroupDisplayItem groupItem)
            {
                group.IsExpanded = true;

                // Get the ListViewItem container for the group
                var groupContainer = ListViewItems.ItemContainerGenerator.ContainerFromItem(groupItem) as System.Windows.Controls.ListViewItem;
                if (groupContainer == null)
                    return;

                // Search visual tree for the Expander
                var expander = FindVisualChild<Expander>(groupContainer);
                if (expander == null)
                    return;

                // Search inside the Expander for the inner ListView
                var innerListView = FindVisualChild<System.Windows.Controls.ListView>(expander);
                if (innerListView != null)
                {
                    innerListView.ScrollIntoView(displayItem);
                }
            }
            else
            {
                ListViewItems.ScrollIntoView(displayItem);
            }
        }

        private void ButtonPushUp_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem is DisplayItem item)
            {
                _isHandlingSelection = true;

                try
                {
                    if (item is GroupDisplayItem groupDisplayItem)
                    {
                        foreach (var displayItem in groupDisplayItem.DisplayItems)
                        {
                            displayItem.Selected = false;
                        }
                    }

                    SharedModel.Instance.PushDisplayItemBy(item, -1);
                    ScrollToView(item);


                }
                finally
                {
                    _isHandlingSelection = false;
                }
            }
        }

        private void ButtonPushDown_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem is DisplayItem item)
            {
                _isHandlingSelection = true;

                try
                {
                    if (item is GroupDisplayItem groupDisplayItem)
                    {
                        foreach (var displayItem in groupDisplayItem.DisplayItems)
                        {
                            displayItem.Selected = false;
                        }
                    }

                    SharedModel.Instance.PushDisplayItemBy(item, 1);
                    ScrollToView(item);
                }
                finally
                {
                    _isHandlingSelection = false;
                }
            }
        }

        private void ButtonPushBack_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem is DisplayItem item)
            {
                _isHandlingSelection = true;

                try
                {
                    if (item is GroupDisplayItem groupDisplayItem)
                    {
                        foreach (var displayItem in groupDisplayItem.DisplayItems)
                        {
                            displayItem.Selected = false;
                        }
                    }

                    SharedModel.Instance.PushDisplayItemToTop(item);
                    ScrollToView(item);
                }
                finally
                {
                    _isHandlingSelection = false;
                }
            }
        }

        private void ButtonPushFront_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem is DisplayItem item)
            {
                _isHandlingSelection = true;

                try
                {
                    if (item is GroupDisplayItem groupDisplayItem)
                    {
                        foreach (var displayItem in groupDisplayItem.DisplayItems)
                        {
                            displayItem.Selected = false;
                        }
                    }

                    SharedModel.Instance.PushDisplayItemToEnd(item);
                    ScrollToView(item);
                }
                finally
                {
                    _isHandlingSelection = false;
                }
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem != null)
            {
                SharedModel.Instance.RemoveDisplayItem(SelectedItem);
            }
        }

        private async void ButtonReload_Click(object sender, RoutedEventArgs e)
        {
            await SharedModel.Instance.ReloadDisplayItems();
            _displayItemsViewSource?.View?.Refresh();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            ConfigModel.Instance.SaveProfiles();
            SharedModel.Instance.SaveDisplayItems();
        }

        private void ButtonNewText_Click(object sender, RoutedEventArgs e)
        {
            if (SharedModel.Instance.SelectedProfile is Profile selectedProfile)
            {
                var item = new TextDisplayItem("Custom Text", selectedProfile)
                {
                    Font = SharedModel.Instance.SelectedProfile!.Font,
                    FontSize = SharedModel.Instance.SelectedProfile!.FontSize,
                    Color = SharedModel.Instance.SelectedProfile!.Color
                };
                SharedModel.Instance.AddDisplayItem(item);
            }


        }

        private void ButtonNewImage_Click(object sender, RoutedEventArgs e)
        {
            if (SharedModel.Instance.SelectedProfile != null)
            {
                var item = new ImageDisplayItem("Image", SharedModel.Instance.SelectedProfile);
                SharedModel.Instance.AddDisplayItem(item);
            }
        }

        private void ButtonNewClock_Click(object sender, RoutedEventArgs e)
        {
            if (SharedModel.Instance.SelectedProfile is Profile selectedProfile)
            {
                var item = new ClockDisplayItem("Clock", selectedProfile)
                {
                    Font = SharedModel.Instance.SelectedProfile!.Font,
                    FontSize = SharedModel.Instance.SelectedProfile!.FontSize,
                    Color = SharedModel.Instance.SelectedProfile!.Color

                };
                SharedModel.Instance.AddDisplayItem(item);
            }
        }

        private void ButtonNewCalendar_Click(object sender, RoutedEventArgs e)
        {
            if (SharedModel.Instance.SelectedProfile is Profile selectedProfile)
            {
                var item = new CalendarDisplayItem("Calendar", selectedProfile)
                {
                    Font = SharedModel.Instance.SelectedProfile!.Font,
                    FontSize = SharedModel.Instance.SelectedProfile!.FontSize,
                    Color = SharedModel.Instance.SelectedProfile!.Color
                };
                SharedModel.Instance.AddDisplayItem(item);
            }

        }

        private void ButtonDuplicate_Click(object sender, RoutedEventArgs e)
        {
            if (SharedModel.Instance.SelectedItem is DisplayItem selectedItem)
            {
                var item = (DisplayItem)selectedItem.Clone();
                SharedModel.Instance.AddDisplayItem(item);
                SharedModel.Instance.PushDisplayItemTo(item, selectedItem);
                item.Selected = true;
            }
        }

        private bool _isHandlingSelection;

        private void ListViewItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isHandlingSelection || sender is not ListView listView)
                return;

            Logger.Debug("ListViewItems_SelectionChanged - {Count} SelectedItems", listView.SelectedItems.Count);

            _isHandlingSelection = true;
            try
            {
                if (listView.SelectedItems.Count == 0)
                {
                    return;
                }

                foreach (var selectedItem in listView.SelectedItems)
                {
                    if (selectedItem is GroupDisplayItem groupDisplayItem)
                    {
                        foreach (var item in groupDisplayItem.DisplayItems)
                        {
                            item.Selected = true;
                        }
                    }
                }

                var selectedItems = listView.SelectedItems.Cast<DisplayItem>().ToList();

                if (selectedItems.Count != 0)
                    listView.ScrollIntoView(selectedItems.Last());

                // Check for modifier keys in WinUI 3
                var ctrlKey = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
                var shiftKey = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift);

                if ((ctrlKey & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down ||
                    (shiftKey & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
                {
                    return;
                }

                SharedModel.Instance.AccessDisplayItems(displayItems => {
                    foreach (var item in displayItems)
                    {
                        if (item != listView.SelectedItem)
                        {
                            if (item is GroupDisplayItem group)
                            {
                                foreach (var item1 in group.DisplayItems)
                                {
                                    item1.Selected = false;
                                }
                            }
                            else
                            {
                                item.Selected = false;
                            }
                        }
                    }
                });
            }
            finally
            {
                SharedModel.Instance.NotifySelectedItemChange();
                _isHandlingSelection = false;
                Logger.Debug("ListViewItems_SelectionChanged - finally");
            }
        }

        private void InnerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isHandlingSelection || sender is not ListView innerListView)
                return;

            Log.Debug("InnerListView_SelectionChanged - {Count} SelectedItems", innerListView.SelectedItems.Count);

            _isHandlingSelection = true;

            try
            {
                var selectedItems = innerListView.SelectedItems.Cast<DisplayItem>().ToList();

                if (selectedItems.Count != 0)
                    innerListView.ScrollIntoView(selectedItems.Last());

                // Check for modifier keys in WinUI 3
                var ctrlKey = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
                var shiftKey = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift);

                if ((ctrlKey & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down ||
                    (shiftKey & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
                {
                    return;
                }

                foreach (var item in SharedModel.Instance.SelectedItems)
                {
                    if (!innerListView.SelectedItems.Contains(item))
                    {
                        item.Selected = false;
                    }
                }

            }
            finally
            {
                SharedModel.Instance.NotifySelectedItemChange();
                _isHandlingSelection = false;
                Log.Debug("InnerListView_SelectionChanged - finally");
            }
        }

        private void Border_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // Handle group selection in WinUI 3
            if (sender is Border border && border.DataContext is DisplayItem item)
            {
                // Clear other selections and select this item
                SharedModel.Instance.AccessDisplayItems(displayItems =>
                {
                    foreach (var displayItem in displayItems)
                    {
                        if (displayItem is GroupDisplayItem group)
                        {
                            foreach (var groupItem in group.DisplayItems)
                            {
                                groupItem.Selected = false;
                            }
                        }
                        else
                        {
                            displayItem.Selected = false;
                        }
                    }
                });
                
                item.Selected = true;
                SharedModel.Instance.NotifySelectedItemChange();
            }
        }

        // WinUI 3 Drag-Drop Event Handlers
        private void ListViewItems_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
            e.DragUIOverride.Caption = "Move item";
        }

        private void ListViewItems_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                var text = e.DataView.GetTextAsync().GetResults();
                // Handle drop logic here
            }
        }

        private void InnerListView_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
            e.DragUIOverride.Caption = "Move to group";
        }

        private void InnerListView_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                var text = e.DataView.GetTextAsync().GetResults();
                // Handle drop logic for inner list view
            }
        }

        }
}
