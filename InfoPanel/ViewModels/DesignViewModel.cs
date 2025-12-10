using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfoPanel.Models;
using InfoPanel.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input; // ICommand interface - shared between WPF and WinUI

namespace InfoPanel.ViewModels
{
    public partial class DesignViewModel : ObservableObject
    {
        private readonly NavigationService _navigationService;
        private ICommand? _navigateCommand;

        public ICommand NavigateCommand => _navigateCommand ??= new RelayCommand<string>(OnNavigate);

        public DesignViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        private void OnNavigate(string? parameter)
        {
            switch (parameter)
            {
                case "navigate_to_profiles":
                    _navigationService.Navigate(typeof(Views.Pages.ProfilesPage));
                    return;
                case "navigate_to_design":
                    _navigationService.Navigate(typeof(Views.Pages.DesignPage));
                    return;
                case "navigate_to_updates":
                    _navigationService.Navigate(typeof(Views.Pages.UpdatesPage));
                    return;
                case "navigate_to_about":
                    _navigationService.Navigate(typeof(Views.Pages.AboutPage));
                    return;
                case "navigate_to_settings":
                    _navigationService.Navigate(typeof(Views.Pages.SettingsPage));
                    return;
                default:
                    _navigationService.Navigate(typeof(Views.Pages.HomePage));
                    return;
            }
        }
    }
}
