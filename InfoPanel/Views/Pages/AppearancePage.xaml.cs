using InfoPanel.ViewModels;
using System.Windows.Controls;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace InfoPanel.Views.Pages
{
    /// <summary>
    /// Interaction logic for AppearancePage.xaml
    /// </summary>
    public partial class AppearancePage : Page, INavigableView<AppearanceViewModel>
    {
        public AppearanceViewModel ViewModel { get; }

        public AppearancePage(AppearanceViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }
    }
}
