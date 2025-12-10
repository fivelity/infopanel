using CommunityToolkit.Mvvm.Input;
using InfoPanel.Models;
using InfoPanel.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace InfoPanel.Views.Pages
{
    public sealed partial class DesignPage : Page
    {
        public DesignViewModel ViewModel { get; }

        public DesignPage()
        {
            ViewModel = new DesignViewModel();
            this.InitializeComponent();
            this.Unloaded += DesignPage_Unloaded;
        }

        public DesignPage(DesignViewModel viewModel)
        {
            ViewModel = viewModel;
            this.InitializeComponent();
            this.Unloaded += DesignPage_Unloaded;
        }

        private void DesignPage_Unloaded(object sender, RoutedEventArgs _)
        {
            SharedModel.Instance.SelectedItem = null;
        }

        [RelayCommand]
        private static void Unselect()
        {
            if (SharedModel.Instance.SelectedItem is DisplayItem selectedItem)
            {
                selectedItem.Selected = false;
            }
        }
    }
}
