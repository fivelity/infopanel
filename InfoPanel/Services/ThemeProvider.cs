using InfoPanel.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace InfoPanel.Services
{
    /// <summary>
    /// Service for managing themes. Loads theme JSON files, applies tokenized ResourceDictionaries,
    /// and handles theme switching.
    /// </summary>
    public class ThemeProvider
    {
        private static ThemeProvider? _instance;
        public static ThemeProvider Instance => _instance ??= new ThemeProvider();

        private readonly Dictionary<string, ThemeModel> _themes = new();
        private string _currentThemeId = "dark-default";
        private readonly string _themesDirectory;

        public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

        public ThemeProvider()
        {
            // Get themes directory from AppData
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _themesDirectory = Path.Combine(appDataPath, "InfoPanel", "Themes");

            // Create directory if it doesn't exist
            if (!Directory.Exists(_themesDirectory))
            {
                Directory.CreateDirectory(_themesDirectory);
            }
        }

        /// <summary>
        /// Current theme ID
        /// </summary>
        public string CurrentThemeId
        {
            get => _currentThemeId;
            set
            {
                if (_currentThemeId != value)
                {
                    _currentThemeId = value;
                    ApplyTheme(value);
                }
            }
        }

        /// <summary>
        /// Get the current theme
        /// </summary>
        public ThemeModel? CurrentTheme => _themes.TryGetValue(_currentThemeId, out var theme) ? theme : null;

        /// <summary>
        /// Get all available themes
        /// </summary>
        public IReadOnlyDictionary<string, ThemeModel> Themes => _themes;

        /// <summary>
        /// Load all themes from the themes directory
        /// </summary>
        public async Task LoadThemesAsync()
        {
            _themes.Clear();

            // Load built-in default themes
            LoadDefaultThemes();

            // Load custom themes from directory
            if (Directory.Exists(_themesDirectory))
            {
                var themeFiles = Directory.GetFiles(_themesDirectory, "*.json", SearchOption.AllDirectories);
                foreach (var file in themeFiles)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(file);
                        var theme = JsonSerializer.Deserialize<ThemeModel>(json);
                        if (theme != null && !string.IsNullOrEmpty(theme.Id))
                        {
                            _themes[theme.Id] = theme;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error (could integrate with existing logging system)
                        Console.WriteLine($"Error loading theme from {file}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Apply a theme by ID
        /// </summary>
        public void ApplyTheme(string themeId)
        {
            if (!_themes.TryGetValue(themeId, out var theme))
            {
                // Fall back to default theme if not found
                themeId = "dark-default";
                if (!_themes.TryGetValue(themeId, out theme))
                {
                    return;
                }
            }

            // Create ResourceDictionary from theme
            var resourceDict = CreateResourceDictionary(theme);

            // Apply to application resources
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Remove old theme resources
                var oldThemeDict = Application.Current.Resources.MergedDictionaries
                    .FirstOrDefault(d => d.Contains("ThemeId"));
                if (oldThemeDict != null)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(oldThemeDict);
                }

                // Add new theme resources
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            });

            // Raise event
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(theme));
        }

        /// <summary>
        /// Create a ResourceDictionary from a theme model
        /// </summary>
        private ResourceDictionary CreateResourceDictionary(ThemeModel theme)
        {
            var dict = new ResourceDictionary();

            // Add theme ID for tracking
            dict["ThemeId"] = theme.Id;

            // Add color tokens
            dict["ThemePrimaryColor"] = theme.Colors.Primary;
            dict["ThemePrimaryHoverColor"] = theme.Colors.PrimaryHover;
            dict["ThemePrimaryPressedColor"] = theme.Colors.PrimaryPressed;
            dict["ThemeBackgroundColor"] = theme.Colors.Background;
            dict["ThemeBackgroundSecondaryColor"] = theme.Colors.BackgroundSecondary;
            dict["ThemeBackgroundTertiaryColor"] = theme.Colors.BackgroundTertiary;
            dict["ThemeSurfaceColor"] = theme.Colors.Surface;
            dict["ThemeSurfaceHoverColor"] = theme.Colors.SurfaceHover;
            dict["ThemeSurfacePressedColor"] = theme.Colors.SurfacePressed;
            dict["ThemeTextPrimaryColor"] = theme.Colors.TextPrimary;
            dict["ThemeTextSecondaryColor"] = theme.Colors.TextSecondary;
            dict["ThemeTextTertiaryColor"] = theme.Colors.TextTertiary;
            dict["ThemeTextDisabledColor"] = theme.Colors.TextDisabled;
            dict["ThemeAccentColor"] = theme.Colors.Accent;
            dict["ThemeAccentHoverColor"] = theme.Colors.AccentHover;
            dict["ThemeSuccessColor"] = theme.Colors.Success;
            dict["ThemeWarningColor"] = theme.Colors.Warning;
            dict["ThemeErrorColor"] = theme.Colors.Error;
            dict["ThemeInfoColor"] = theme.Colors.Info;
            dict["ThemeBorderColor"] = theme.Colors.Border;
            dict["ThemeBorderHoverColor"] = theme.Colors.BorderHover;
            dict["ThemeDividerColor"] = theme.Colors.Divider;

            // Add typography tokens
            dict["ThemeFontFamily"] = theme.Typography.FontFamily;
            dict["ThemeFontFamilyMono"] = theme.Typography.FontFamilyMono;
            dict["ThemeFontSizeXs"] = theme.Typography.FontSizeXs;
            dict["ThemeFontSizeSm"] = theme.Typography.FontSizeSm;
            dict["ThemeFontSizeMd"] = theme.Typography.FontSizeMd;
            dict["ThemeFontSizeLg"] = theme.Typography.FontSizeLg;
            dict["ThemeFontSizeXl"] = theme.Typography.FontSizeXl;
            dict["ThemeFontSize2Xl"] = theme.Typography.FontSize2Xl;
            dict["ThemeFontSize3Xl"] = theme.Typography.FontSize3Xl;

            // Add spacing tokens
            dict["ThemeSpacingXs"] = theme.Spacing.Xs;
            dict["ThemeSpacingSm"] = theme.Spacing.Sm;
            dict["ThemeSpacingMd"] = theme.Spacing.Md;
            dict["ThemeSpacingLg"] = theme.Spacing.Lg;
            dict["ThemeSpacingXl"] = theme.Spacing.Xl;
            dict["ThemeSpacing2Xl"] = theme.Spacing.Xl2;
            dict["ThemeSpacing3Xl"] = theme.Spacing.Xl3;
            dict["ThemeRadiusSm"] = theme.Spacing.RadiusSm;
            dict["ThemeRadiusMd"] = theme.Spacing.RadiusMd;
            dict["ThemeRadiusLg"] = theme.Spacing.RadiusLg;
            dict["ThemeRadiusXl"] = theme.Spacing.RadiusXl;
            dict["ThemeRadiusFull"] = theme.Spacing.RadiusFull;

            // Add elevation tokens
            dict["ThemeShadow1"] = theme.Elevation.Shadow1;
            dict["ThemeShadow2"] = theme.Elevation.Shadow2;
            dict["ThemeShadow3"] = theme.Elevation.Shadow3;
            dict["ThemeShadow4"] = theme.Elevation.Shadow4;
            dict["ThemeShadow5"] = theme.Elevation.Shadow5;
            dict["ThemeZIndexBase"] = theme.Elevation.ZIndexBase;
            dict["ThemeZIndexDropdown"] = theme.Elevation.ZIndexDropdown;
            dict["ThemeZIndexModal"] = theme.Elevation.ZIndexModal;
            dict["ThemeZIndexTooltip"] = theme.Elevation.ZIndexTooltip;

            // Add motion tokens
            dict["ThemeDurationFast"] = theme.Motion.DurationFast;
            dict["ThemeDurationNormal"] = theme.Motion.DurationNormal;
            dict["ThemeDurationSlow"] = theme.Motion.DurationSlow;

            return dict;
        }

        /// <summary>
        /// Load built-in default themes
        /// </summary>
        private void LoadDefaultThemes()
        {
            // Dark theme (default)
            _themes["dark-default"] = new ThemeModel
            {
                Id = "dark-default",
                Name = "Dark (Default)",
                Description = "Default dark theme for InfoPanel",
                Author = "InfoPanel Team",
                Version = "1.0.0",
                Colors = new ColorTokens
                {
                    Primary = "#0078D4",
                    PrimaryHover = "#106EBE",
                    PrimaryPressed = "#005A9E",
                    Background = "#202020",
                    BackgroundSecondary = "#2C2C2C",
                    BackgroundTertiary = "#1A1A1A",
                    Surface = "#2C2C2C",
                    SurfaceHover = "#3C3C3C",
                    SurfacePressed = "#1C1C1C",
                    TextPrimary = "#FFFFFF",
                    TextSecondary = "#B3B3B3",
                    TextTertiary = "#808080",
                    TextDisabled = "#5C5C5C",
                    Accent = "#0078D4",
                    AccentHover = "#106EBE",
                    Success = "#10893E",
                    Warning = "#F7630C",
                    Error = "#E81123",
                    Info = "#0078D4",
                    Border = "#404040",
                    BorderHover = "#606060",
                    Divider = "#333333"
                }
            };

            // Light theme
            _themes["light-default"] = new ThemeModel
            {
                Id = "light-default",
                Name = "Light",
                Description = "Light theme for InfoPanel",
                Author = "InfoPanel Team",
                Version = "1.0.0",
                Colors = new ColorTokens
                {
                    Primary = "#0078D4",
                    PrimaryHover = "#106EBE",
                    PrimaryPressed = "#005A9E",
                    Background = "#FFFFFF",
                    BackgroundSecondary = "#F5F5F5",
                    BackgroundTertiary = "#FAFAFA",
                    Surface = "#FFFFFF",
                    SurfaceHover = "#F5F5F5",
                    SurfacePressed = "#E0E0E0",
                    TextPrimary = "#000000",
                    TextSecondary = "#5C5C5C",
                    TextTertiary = "#808080",
                    TextDisabled = "#B3B3B3",
                    Accent = "#0078D4",
                    AccentHover = "#106EBE",
                    Success = "#10893E",
                    Warning = "#F7630C",
                    Error = "#E81123",
                    Info = "#0078D4",
                    Border = "#D0D0D0",
                    BorderHover = "#A0A0A0",
                    Divider = "#E0E0E0"
                }
            };
        }

        /// <summary>
        /// Save a theme to a JSON file
        /// </summary>
        public async Task SaveThemeAsync(ThemeModel theme)
        {
            var filePath = Path.Combine(_themesDirectory, $"{theme.Id}.json");
            var json = JsonSerializer.Serialize(theme, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
            _themes[theme.Id] = theme;
        }

        /// <summary>
        /// Export a theme to a file
        /// </summary>
        public async Task ExportThemeAsync(string themeId, string filePath)
        {
            if (_themes.TryGetValue(themeId, out var theme))
            {
                var json = JsonSerializer.Serialize(theme, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, json);
            }
        }

        /// <summary>
        /// Import a theme from a file
        /// </summary>
        public async Task<ThemeModel?> ImportThemeAsync(string filePath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var theme = JsonSerializer.Deserialize<ThemeModel>(json);
                if (theme != null && !string.IsNullOrEmpty(theme.Id))
                {
                    await SaveThemeAsync(theme);
                    return theme;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing theme: {ex.Message}");
            }
            return null;
        }
    }

    /// <summary>
    /// Event args for theme changed event
    /// </summary>
    public class ThemeChangedEventArgs : EventArgs
    {
        public ThemeModel Theme { get; }

        public ThemeChangedEventArgs(ThemeModel theme)
        {
            Theme = theme;
        }
    }
}
