using CommunityToolkit.Mvvm.ComponentModel;
using InfoPanel.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace InfoPanel.ViewModels
{
    public class ProfilesViewModel: ObservableObject
    {
        private Profile? _profile;

        public Profile? Profile
        {
            get { return _profile; }
            set
            {
                SetProperty(ref _profile, value);
            }
        }

        public ProfilesViewModel()
        {
        }
    }
}
