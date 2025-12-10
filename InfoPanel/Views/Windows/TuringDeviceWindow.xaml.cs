using InfoPanel.Models;
using InfoPanel.ViewModels;
using Microsoft.UI.Xaml;

namespace InfoPanel.Views.Windows;

public sealed partial class TuringDeviceWindow : Window
{
    private readonly TuringDeviceWindowViewModel _viewModel;

    public TuringDeviceWindow(TuringPanelDevice device)
    {
        this.InitializeComponent();

        _viewModel = new TuringDeviceWindowViewModel(device);
        
        if (Content is FrameworkElement content)
        {
            content.DataContext = _viewModel;
        }

        this.Closed += (s, e) => _viewModel.Cleanup();
    }

    public void ShowDialog()
    {
        // WinUI 3 doesn't have ShowDialog - activate the window instead
        this.Activate();
    }
}