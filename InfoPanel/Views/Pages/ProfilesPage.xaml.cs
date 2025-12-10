using InfoPanel.Models;
using InfoPanel.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage.Pickers;
using Windows.UI;

namespace InfoPanel.Views.Pages
{
    public sealed partial class ProfilesPage : Page
    {
        public ObservableCollection<string> InstalledFonts { get; } = [];
        public ProfilesViewModel ViewModel { get; }
        private Profile? _selectedProfile;

        public ProfilesPage()
        {
            ViewModel = new ProfilesViewModel();
            LoadAllFonts();
            this.InitializeComponent();
            LoadProfiles();
        }

        public ProfilesPage(ProfilesViewModel viewModel)
        {
            ViewModel = viewModel;
            LoadAllFonts();
            this.InitializeComponent();
            LoadProfiles();
        }

        private void LoadAllFonts()
        {
            var allFonts = SKFontManager.Default.GetFontFamilies()
                .OrderBy(f => f)
                .ToList();

            foreach (var font in allFonts)
            {
                InstalledFonts.Add(font);
                ComboBoxFont.Items.Add(font);
            }
        }

        private void LoadProfiles()
        {
            GridViewProfiles.ItemsSource = ConfigModel.Instance.Profiles;
        }

        private void GridViewProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridViewProfiles.SelectedItem is Profile profile)
            {
                _selectedProfile = profile;
                ViewModel.Profile = profile;
                ShowProfileEditor(profile);
            }
            else
            {
                _selectedProfile = null;
                ViewModel.Profile = null;
                HideProfileEditor();
            }
        }

        private void ShowProfileEditor(Profile profile)
        {
            GridProfileEditor.Visibility = Visibility.Visible;
            BorderGuide.Visibility = Visibility.Collapsed;
            TextBlockProfileHeader.Text = $"{profile.Name} Properties";

            // Load profile data into controls
            TextBoxName.Text = profile.Name;
            TextBlockTargetWindow.Text = profile.TargetWindow?.Name ?? "No target";
            TextBlockDeviceName.Text = profile.TargetWindow?.DeviceName ?? "";

            ToggleStrictMatching.IsChecked = profile.StrictWindowMatching;
            ToggleTopmost.IsChecked = profile.Topmost;
            ToggleDrag.IsChecked = profile.Drag;
            ToggleResize.IsChecked = profile.Resize;

            NumberBoxWidth.Value = profile.Width;
            NumberBoxHeight.Value = profile.Height;
            NumberBoxX.Value = profile.WindowX;
            NumberBoxY.Value = profile.WindowY;

            // Colors - convert System.Drawing.Color to Windows.UI.Color
            ColorPickerBackground.Color = Color.FromArgb(profile.BackgroundColor.A, profile.BackgroundColor.R, profile.BackgroundColor.G, profile.BackgroundColor.B);
            ColorPickerText.Color = Color.FromArgb(profile.Color.A, profile.Color.R, profile.Color.G, profile.Color.B);

            // Font
            for (int i = 0; i < ComboBoxFont.Items.Count; i++)
            {
                if (ComboBoxFont.Items[i]?.ToString() == profile.Font)
                {
                    ComboBoxFont.SelectedIndex = i;
                    break;
                }
            }

            NumberBoxFontSize.Value = profile.FontSize;
            ToggleShowFps.IsOn = profile.ShowFps;
            ToggleOpenGL.IsOn = profile.OpenGL;
            NumberBoxFontScale.Value = profile.FontScale;

            // Wire up change events
            TextBoxName.TextChanged += (s, e) => { if (_selectedProfile != null) _selectedProfile.Name = TextBoxName.Text; };
            ToggleStrictMatching.Click += (s, e) => { if (_selectedProfile != null) _selectedProfile.StrictWindowMatching = ToggleStrictMatching.IsChecked ?? false; };
            ToggleTopmost.Click += (s, e) => { if (_selectedProfile != null) _selectedProfile.Topmost = ToggleTopmost.IsChecked ?? false; };
            ToggleDrag.Click += (s, e) => { if (_selectedProfile != null) _selectedProfile.Drag = ToggleDrag.IsChecked ?? false; };
            ToggleResize.Click += (s, e) => { if (_selectedProfile != null) _selectedProfile.Resize = ToggleResize.IsChecked ?? false; };
            NumberBoxWidth.ValueChanged += (s, e) => { if (_selectedProfile != null) _selectedProfile.Width = (int)NumberBoxWidth.Value; };
            NumberBoxHeight.ValueChanged += (s, e) => { if (_selectedProfile != null) _selectedProfile.Height = (int)NumberBoxHeight.Value; };
            NumberBoxX.ValueChanged += (s, e) => { if (_selectedProfile != null) _selectedProfile.WindowX = (int)NumberBoxX.Value; };
            NumberBoxY.ValueChanged += (s, e) => { if (_selectedProfile != null) _selectedProfile.WindowY = (int)NumberBoxY.Value; };
            NumberBoxFontSize.ValueChanged += (s, e) => { if (_selectedProfile != null) _selectedProfile.FontSize = (int)NumberBoxFontSize.Value; };
            NumberBoxFontScale.ValueChanged += (s, e) => { if (_selectedProfile != null) _selectedProfile.FontScale = NumberBoxFontScale.Value; };
            ToggleShowFps.Toggled += (s, e) => { if (_selectedProfile != null) _selectedProfile.ShowFps = ToggleShowFps.IsOn; };
            ToggleOpenGL.Toggled += (s, e) => { if (_selectedProfile != null) _selectedProfile.OpenGL = ToggleOpenGL.IsOn; };
            ComboBoxFont.SelectionChanged += (s, e) => { if (_selectedProfile != null && ComboBoxFont.SelectedItem != null) _selectedProfile.Font = ComboBoxFont.SelectedItem.ToString()!; };

            ColorPickerBackground.ColorChanged += (s, e) =>
            {
                if (_selectedProfile != null)
                {
                    var c = ColorPickerBackground.Color;
                    _selectedProfile.BackgroundColor = System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
                }
            };

            ColorPickerText.ColorChanged += (s, e) =>
            {
                if (_selectedProfile != null)
                {
                    var c = ColorPickerText.Color;
                    _selectedProfile.Color = System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
                }
            };
        }

        private void HideProfileEditor()
        {
            GridProfileEditor.Visibility = Visibility.Collapsed;
            BorderGuide.Visibility = Visibility.Visible;
            TextBlockProfileHeader.Text = "Select a profile";
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            var profile = new Profile()
            {
                Name = "Profile " + (ConfigModel.Instance.Profiles.Count + 1)
            };
            ConfigModel.Instance.AddProfile(profile);
            ConfigModel.Instance.SaveProfiles();
            SharedModel.Instance.SaveDisplayItems(profile);
            ViewModel.Profile = profile;
            GridViewProfiles.SelectedItem = profile;
        }

        private async void ButtonImportProfile_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".infopanel");
            picker.FileTypeFilter.Add(".sensorpanel");
            picker.FileTypeFilter.Add(".rslcd");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.GetService<Views.Windows.MainWindow>());
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                if (file.Path.EndsWith(".infopanel"))
                {
                    SharedModel.Instance.ImportProfile(file.Path);
                }
                else if (file.Path.EndsWith(".sensorpanel") || file.Path.EndsWith(".rslcd"))
                {
                    await SharedModel.ImportSensorPanel(file.Path);
                }
                LoadProfiles();
            }
        }

        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProfile is Profile profile)
            {
                ConfigModel.Instance.SaveProfiles();
                SharedModel.Instance.SaveDisplayItems(profile);

                var dialog = new ContentDialog
                {
                    Title = "Profile Saved",
                    Content = $"{profile.Name} has been saved.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private void ButtonResetPosition_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProfile is Profile profile)
            {
                profile.WindowX = 0;
                profile.WindowY = 0;
                NumberBoxX.Value = 0;
                NumberBoxY.Value = 0;
            }
        }

        private void ButtonMaximise_Click(object sender, RoutedEventArgs e)
        {
            if (App.Current is App app && _selectedProfile is Profile profile)
            {
                app.MaximiseDisplayWindow(profile);
            }
        }

        private void ButtonReload_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProfile is Profile profile)
            {
                ConfigModel.Instance.ReloadProfile(profile);
                ShowProfileEditor(profile);
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            _selectedProfile = null;
            ViewModel.Profile = null;
            GridViewProfiles.SelectedItem = null;
            HideProfileEditor();
        }
    }
}
