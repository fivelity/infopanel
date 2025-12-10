using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfoPanel.Models;
using InfoPanel.Plugins;
using InfoPanel.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace InfoPanel.ViewModels
{
    /// <summary>
    /// ViewModel for the Appearance page. Manages themes, layouts, and workspaces.
    /// </summary>
    public partial class AppearanceViewModel : ObservableObject, INavigationAware
    {
        private readonly ThemeProvider _themeProvider;
        private readonly LayoutProvider _layoutProvider;
        private readonly WorkspaceManager _workspaceManager;

        [ObservableProperty]
        private ObservableCollection<ThemeDisplayModel> _availableThemes = new();

        [ObservableProperty]
        private ObservableCollection<LayoutDisplayModel> _availableLayouts = new();

        [ObservableProperty]
        private ObservableCollection<WorkspaceDisplayModel> _workspaces = new();

        [ObservableProperty]
        private string? _selectedThemeId;

        [ObservableProperty]
        private string? _selectedLayoutId;

        [ObservableProperty]
        private bool _hasSelectedTheme;

        [ObservableProperty]
        private bool _hasSelectedLayout;

        // Commands
        public ICommand SelectThemeCommand { get; }
        public ICommand ApplyThemeCommand { get; }
        public ICommand ImportThemeCommand { get; }
        public ICommand ExportThemeCommand { get; }

        public ICommand SelectLayoutCommand { get; }
        public ICommand ApplyLayoutCommand { get; }
        public ICommand ImportLayoutCommand { get; }
        public ICommand ExportLayoutCommand { get; }

        public ICommand NewWorkspaceCommand { get; }
        public ICommand LoadWorkspaceCommand { get; }
        public ICommand SaveWorkspaceCommand { get; }
        public ICommand DeleteWorkspaceCommand { get; }
        public ICommand ExportWorkspaceCommand { get; }
        public ICommand ImportWorkspaceCommand { get; }
        public ICommand DuplicateWorkspaceCommand { get; }

        public AppearanceViewModel(ThemeProvider themeProvider, LayoutProvider layoutProvider, WorkspaceManager workspaceManager)
        {
            _themeProvider = themeProvider;
            _layoutProvider = layoutProvider;
            _workspaceManager = workspaceManager;

            // Initialize commands
            SelectThemeCommand = new RelayCommand<string>(SelectTheme);
            ImportThemeCommand = new AsyncRelayCommand(ImportThemeAsync);
            ExportThemeCommand = new AsyncRelayCommand(ExportThemeAsync);

            SelectLayoutCommand = new RelayCommand<string>(SelectLayout);
            ImportLayoutCommand = new AsyncRelayCommand(ImportLayoutAsync);
            ExportLayoutCommand = new AsyncRelayCommand(ExportLayoutAsync);

            NewWorkspaceCommand = new AsyncRelayCommand(NewWorkspaceAsync);
            LoadWorkspaceCommand = new RelayCommand<string>(LoadWorkspace);
            SaveWorkspaceCommand = new AsyncRelayCommand<string>(SaveWorkspaceAsync);
            DeleteWorkspaceCommand = new AsyncRelayCommand<string>(DeleteWorkspaceAsync);
            ExportWorkspaceCommand = new AsyncRelayCommand<string>(ExportWorkspaceAsync);
            ImportWorkspaceCommand = new AsyncRelayCommand(ImportWorkspaceAsync);
            DuplicateWorkspaceCommand = new AsyncRelayCommand<string>(DuplicateWorkspaceAsync);

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await _themeProvider.LoadThemesAsync();
            await _layoutProvider.LoadLayoutsAsync();
            await _workspaceManager.LoadWorkspacesAsync();

            LoadThemes();
            LoadLayouts();
            LoadWorkspaces();
        }

        #region Theme Management

        private void LoadThemes()
        {
            AvailableThemes.Clear();
            foreach (var theme in _themeProvider.Themes.Values)
            {
                AvailableThemes.Add(new ThemeDisplayModel(theme)
                {
                    IsSelected = theme.Id == _themeProvider.CurrentThemeId
                });
            }
        }

        private void SelectTheme(string? themeId)
        {
            if (string.IsNullOrEmpty(themeId)) return;

            SelectedThemeId = themeId;
            HasSelectedTheme = true;

            // Apply theme immediately
            try
            {
                _themeProvider.ApplyTheme(themeId);
            }
            catch (Exception ex)
            {
                // Log error or show unobtrusive notification if needed
                System.Diagnostics.Debug.WriteLine($"Error applying theme: {ex.Message}");
            }

            // Update selection visual state
            foreach (var theme in AvailableThemes)
            {
                theme.IsSelected = theme.Id == themeId;
            }
        }

        private async Task ImportThemeAsync()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Import Theme"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var theme = await _themeProvider.ImportThemeAsync(dialog.FileName);
                    if (theme != null)
                    {
                        LoadThemes();
                        MessageBox.Show($"Theme '{theme.Name}' imported successfully!", "Import Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to import theme. Invalid file format.", "Import Failed",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing theme: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task ExportThemeAsync()
        {
            if (string.IsNullOrEmpty(_themeProvider.CurrentThemeId)) return;

            var dialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                FileName = $"{_themeProvider.CurrentThemeId}.json",
                Title = "Export Theme"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    await _themeProvider.ExportThemeAsync(_themeProvider.CurrentThemeId, dialog.FileName);
                    MessageBox.Show("Theme exported successfully!", "Export Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting theme: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Layout Management

        private void LoadLayouts()
        {
            AvailableLayouts.Clear();
            foreach (var layout in _layoutProvider.Layouts.Values)
            {
                AvailableLayouts.Add(new LayoutDisplayModel(layout)
                {
                    IsSelected = layout.Id == _layoutProvider.CurrentLayoutId
                });
            }
        }

        private void SelectLayout(string? layoutId)
        {
            if (string.IsNullOrEmpty(layoutId)) return;

            SelectedLayoutId = layoutId;
            HasSelectedLayout = true;

            // Apply layout immediately
            try
            {
                _layoutProvider.CurrentLayoutId = layoutId;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying layout: {ex.Message}");
            }

            // Update selection visual state
            foreach (var layout in AvailableLayouts)
            {
                layout.IsSelected = layout.Id == layoutId;
            }
        }

        private async Task ImportLayoutAsync()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Import Layout"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var layout = await _layoutProvider.ImportLayoutAsync(dialog.FileName);
                    if (layout != null)
                    {
                        LoadLayouts();
                        MessageBox.Show($"Layout '{layout.Name}' imported successfully!", "Import Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to import layout. Invalid file format.", "Import Failed",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing layout: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task ExportLayoutAsync()
        {
            if (string.IsNullOrEmpty(_layoutProvider.CurrentLayoutId)) return;

            var dialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                FileName = $"{_layoutProvider.CurrentLayoutId}.json",
                Title = "Export Layout"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    await _layoutProvider.ExportLayoutAsync(_layoutProvider.CurrentLayoutId, dialog.FileName);
                    MessageBox.Show("Layout exported successfully!", "Export Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting layout: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Workspace Management

        private void LoadWorkspaces()
        {
            Workspaces.Clear();
            foreach (var workspace in _workspaceManager.Workspaces)
            {
                Workspaces.Add(new WorkspaceDisplayModel(workspace)
                {
                    IsActive = workspace.Id == _workspaceManager.ActiveWorkspaceId,
                    CanDelete = _workspaceManager.Workspaces.Count > 1
                });
            }
        }

        private async Task NewWorkspaceAsync()
        {
            // Simple input dialog (you can create a more sophisticated dialog window if needed)
            var name = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter workspace name:",
                "New Workspace",
                "My Workspace");

            if (!string.IsNullOrWhiteSpace(name))
            {
                try
                {
                    await _workspaceManager.CreateWorkspaceAsync(name);
                    LoadWorkspaces();
                    MessageBox.Show($"Workspace '{name}' created successfully!", "Workspace Created",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating workspace: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadWorkspace(string? workspaceId)
        {
            if (string.IsNullOrEmpty(workspaceId)) return;

            var workspace = _workspaceManager.GetWorkspace(workspaceId);
            if (workspace != null)
            {
                try
                {
                    _workspaceManager.ApplyWorkspace(workspace);
                    _workspaceManager.ActiveWorkspaceId = workspaceId;
                    LoadWorkspaces();
                    LoadThemes();
                    LoadLayouts();
                    // Removed success message box for smoother experience
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading workspace: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task SaveWorkspaceAsync(string? workspaceId)
        {
            if (string.IsNullOrEmpty(workspaceId)) return;

            try
            {
                await _workspaceManager.SaveCurrentStateToWorkspaceAsync(workspaceId);
                MessageBox.Show("Workspace saved successfully!", "Workspace Saved",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving workspace: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteWorkspaceAsync(string? workspaceId)
        {
            if (string.IsNullOrEmpty(workspaceId)) return;

            var result = MessageBox.Show(
                "Are you sure you want to delete this workspace? This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _workspaceManager.DeleteWorkspaceAsync(workspaceId);
                    LoadWorkspaces();
                    MessageBox.Show("Workspace deleted successfully!", "Workspace Deleted",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting workspace: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task ExportWorkspaceAsync(string? workspaceId)
        {
            if (string.IsNullOrEmpty(workspaceId)) return;

            var workspace = _workspaceManager.GetWorkspace(workspaceId);
            if (workspace == null) return;

            var dialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                FileName = $"{workspace.Name}.json",
                Title = "Export Workspace"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    await _workspaceManager.ExportWorkspaceAsync(workspaceId, dialog.FileName);
                    MessageBox.Show("Workspace exported successfully!", "Export Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting workspace: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task ImportWorkspaceAsync()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Import Workspace"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var workspace = await _workspaceManager.ImportWorkspaceAsync(dialog.FileName);
                    if (workspace != null)
                    {
                        LoadWorkspaces();
                        MessageBox.Show($"Workspace '{workspace.Name}' imported successfully!", "Import Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to import workspace. Invalid file format.", "Import Failed",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing workspace: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task DuplicateWorkspaceAsync(string? workspaceId)
        {
            if (string.IsNullOrEmpty(workspaceId)) return;

            var workspace = _workspaceManager.GetWorkspace(workspaceId);
            if (workspace == null) return;

            var name = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter name for duplicated workspace:",
                "Duplicate Workspace",
                $"{workspace.Name} (Copy)");

            if (!string.IsNullOrWhiteSpace(name))
            {
                try
                {
                    await _workspaceManager.DuplicateWorkspaceAsync(workspaceId, name);
                    LoadWorkspaces();
                    MessageBox.Show($"Workspace duplicated successfully as '{name}'!", "Workspace Duplicated",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error duplicating workspace: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        public Task OnNavigatedToAsync()
        {
            // Refresh data when navigating to this page
            LoadThemes();
            LoadLayouts();
            LoadWorkspaces();
            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync()
        {
            // Cleanup if needed
            return Task.CompletedTask;
        }
    }

    #region Display Models

    /// <summary>
    /// Display model for themes with selection state
    /// </summary>
    public partial class ThemeDisplayModel : ObservableObject
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public ColorTokens Colors { get; set; } = new();

        [ObservableProperty]
        private bool _isSelected;

        public ThemeDisplayModel(ThemeModel theme)
        {
            Id = theme.Id;
            Name = theme.Name;
            Description = theme.Description;
            Author = theme.Author;
            Version = theme.Version;
            Colors = theme.Colors;
        }
    }

    /// <summary>
    /// Display model for layouts with selection state
    /// </summary>
    public partial class LayoutDisplayModel : ObservableObject
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public LayoutType Type { get; set; }

        [ObservableProperty]
        private bool _isSelected;

        public LayoutDisplayModel(LayoutModel layout)
        {
            Id = layout.Id;
            Name = layout.Name;
            Description = layout.Description;
            Author = layout.Author;
            Version = layout.Version;
            Type = layout.Type;
        }
    }

    /// <summary>
    /// Display model for workspaces with active state
    /// </summary>
    public partial class WorkspaceDisplayModel : ObservableObject
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ThemeId { get; set; } = string.Empty;
        public string LayoutId { get; set; } = string.Empty;
        public bool IsDefault { get; set; }

        [ObservableProperty]
        private bool _isActive;

        [ObservableProperty]
        private bool _canDelete;

        public WorkspaceDisplayModel(WorkspaceModel workspace)
        {
            Id = workspace.Id;
            Name = workspace.Name;
            Description = workspace.Description;
            ThemeId = workspace.ThemeId;
            LayoutId = workspace.LayoutId;
            IsDefault = workspace.IsDefault;
        }
    }

    #endregion
}
